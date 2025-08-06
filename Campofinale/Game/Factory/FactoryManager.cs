using Campofinale.Database;
using Campofinale.Game.Entities;
using Campofinale.Game.Factory.Components;
using Campofinale.Packets.Sc;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using static Campofinale.Resource.ResourceManager;

namespace Campofinale.Game.Factory
{
    public class FactoryManager
    {
        public Player player;
        public List<FactoryChapter> chapters = new();
        public ObjectId _id;
        public class FactoryData
        {
            public ulong roleId;
            public ObjectId _id;
            public List<FactoryChapter> chapters = new();
        }
        public FactoryManager(Player player)
        {
            this.player = player;
        }
        public void Load()
        {
            FactoryData data = DatabaseManager.db.LoadFactoryData(player.roleId);
            if (data != null)
            {
                _id=data._id;
                chapters = data.chapters;
            }
            if(!ChapterExist("domain_1")) chapters.Add(new FactoryChapter("domain_1", player.roleId));
            if(!ChapterExist("domain_2")) chapters.Add(new FactoryChapter("domain_2", player.roleId));
        }
        public bool ChapterExist(string id)
        {
            return chapters.Find(c=>c.chapterId==id)!=null;
        }
        public void Save()
        {
            DatabaseManager.db.UpsertFactoryData(new FactoryData()
            {
                _id= _id,
                roleId=player.roleId,
                chapters=chapters
            });
        }
        public void ExecOp(CsFactoryOp op, ulong seq)
        {
            FactoryChapter chapter = GetChapter(op.ChapterId);
            if (chapter != null)
            {
                chapter.ExecOp(op, seq);
                
            }
            else
            {
                ScFactoryOpRet ret = new()
                {
                    RetCode = FactoryOpRetCode.Fail,
                    
                };
                player.Send(ScMsgId.ScFactoryOpRet, ret, seq);
            }
        }
        public void Update()
        {
            foreach (FactoryChapter chapter in chapters)
            {
                chapter.Update();
            }
        }
        public FactoryChapter GetChapter(string id)
        {
            return chapters.Find(c=>c.chapterId==id);
        }
    }
    public class FactoryChapter
    {
        public string chapterId;
        public ulong ownerId;
        public List<FactoryNode> nodes=new();
        public uint v = 1;
        public uint compV = 0;
        public int bandwidth = 200;

        public ScFactorySyncChapter ToProto()
        {
            ScFactorySyncChapter chapter = new()
            {
                ChapterId = chapterId,
                Tms = DateTime.UtcNow.ToUnixTimestampMilliseconds()/1000,
                Blackboard = new()
                {
                    Power = new()
                    {
                        PowerGen = 0,
                        PowerSaveMax = 0,
                        PowerSaveCurrent = 0,
                        PowerCost = 0
                    },
                    InventoryNodeId = 1
                },
                Statistic = new()
                {
                    LastDay = new()
                    {

                    },
                    Other = new()
                    {
                        InPowerBuilding = 1
                    }
                },
                PinBoard = new()
                {
                    Cards =
                    {

                    },

                },
            };
            chapter.Blackboard.Power.PowerSaveCurrent = bandwidth;
            domainDataTable[chapterId].levelGroup.ForEach(levelGroup =>
            {
                int grade = GetOwner().sceneManager.GetScene(GetSceneNumIdFromLevelData(levelGroup)).grade;
                LevelGradeInfo sceneGrade = ResourceManager.levelGradeTable[levelGroup].grades.Find(g=>g.grade==grade);
                if (sceneGrade != null)
                {
                    chapter.Blackboard.Power.PowerGen += sceneGrade.bandwidth;
                    chapter.Blackboard.Power.PowerSaveMax += sceneGrade.bandwidth;
                    
                    var scene = new ScdFactorySyncScene()
                    {
                        SceneId = GetSceneNumIdFromLevelData(levelGroup),

                        Bandwidth = new()
                        {
                            Current = 0,
                            Max = sceneGrade.bandwidth,
                            TravelPoleMax = sceneGrade.travelPoleLimit,
                            
                            BattleCurrent = 0,
                            BattleMax = sceneGrade.battleBuildingLimit,
                        },
                        Settlements =
                        {
                            
                        },
                        
                        Panels =
                        {

                        }
                    };
                    int index = 0;
                    LevelScene scen = GetLevelData(GetSceneNumIdFromLevelData(levelGroup));
                    foreach (var reg in scen.levelData.factoryRegions)
                    {
                        foreach (var area in reg.areas)
                        {
                            var lvData = area.levelData.Find(l=>l.level==grade);
                            if (lvData == null)
                            {
                                lvData = area.levelData.Last();
                            }
                            if (lvData.levelBounds.Count > 0)
                            {
                                var bounds = lvData.levelBounds[0];
                                scene.Panels.Add(new ScdFactorySyncScenePanel()
                                {
                                    Index = index,
                                    Level = lvData.level,
                                    MainMesh =
                                    {
                                        new ScdRectInt()
                                        {
                                            X=(int)bounds.start.x,
                                            Z=(int)bounds.start.z,
                                            Y=(int)bounds.start.y,
                                            W=(int)bounds.size.x,
                                            H=(int)bounds.size.y,
                                            L=(int)bounds.size.z,
                                        }
                                    }
                                });
                                index++;
                            }

                        }
                    }
                    chapter.Scenes.Add(scene);
                }

            });
            nodes.ForEach(node =>
            {
                chapter.Nodes.Add(node.ToProto());
            });
            chapter.Maps.AddRange(GetMaps());
            return chapter;
        }
        public List<ScdFactorySyncMap> GetMaps()
        {
            List<ScdFactorySyncMap> maps = new();
            string levelId = domainDataTable[chapterId].levelGroup[0];
            string mapId = GetLevelData(GetSceneNumIdFromLevelData(levelId)).mapIdStr;
            maps.Add(new ScdFactorySyncMap()
            {
                MapId = ResourceManager.strIdNumTable.chapter_map_id.dic[mapId],
                Wires =
                {
                    GetWires()
                }
            });
            return maps;
        }
        
        public List<ScdFactorySyncMapWire> GetWires()
        {
            List<ScdFactorySyncMapWire> wires = new();
            HashSet<(ulong, ulong)> addedConnections = new(); // evita doppioni esatti
            ulong i = 0;

            foreach (FactoryNode node in nodes)
            {
                foreach (var conn in node.connectedComps)
                {
                    ulong compA = conn.Key;
                    ulong compB = conn.Value;

                    var key = (compA, compB);

                    if (!addedConnections.Contains(key))
                    {
                        wires.Add(new ScdFactorySyncMapWire()
                        {
                            Index = i,
                            FromComId = compA,
                            ToComId = compB
                        });

                        addedConnections.Add(key);
                        i++;
                    }
                }
            }

            return wires;
        }


        public void Update()
        {
            try
            {
                UpdatePowerGrid(nodes);
                foreach (FactoryNode node in nodes)
                {
                    node.Update(this);
                }
            }
            catch (Exception e)
            {

            }

        }
        public List<FactoryNode> GetNodesInRange(Vector3f pos,float range)
        {
            return nodes.FindAll(n => n.position.Distance(pos) <= range);
        }
        
        public void ExecOp(CsFactoryOp op, ulong seq)
        {
            
            switch (op.OpType)
            {
                case FactoryOpType.Place:
                    CreateNode(op, seq);
                    break;
                case FactoryOpType.MoveNode:
                    MoveNode(op, seq);
                    break;
                case FactoryOpType.Dismantle:
                    DismantleNode(op, seq);
                    break;
                case FactoryOpType.AddConnection:
                    AddConnection(op, seq);
                    break;
                case FactoryOpType.SetTravelPoleDefaultNext:
                    FactoryNode travelNode = GetNodeByCompId(op.SetTravelPoleDefaultNext.ComponentId);
                    travelNode.GetComponent<FComponentTravelPole>().defaultNext = op.SetTravelPoleDefaultNext.DefaultNext;
                    GetOwner().Send(new PacketScFactoryModifyChapterNodes(GetOwner(), chapterId, travelNode));
                    GetOwner().Send(new PacketScFactoryOpRet(GetOwner(), 0, op), seq);
                    break;
                default:
                    break;
            }
                
        }
        public void MoveNode(CsFactoryOp op, ulong seq)
        {
            var move = op.MoveNode;
            FactoryNode node = nodes.Find(n => n.nodeId == move.NodeId);
            if (node != null)
            {
                node.direction = new Vector3f(move.Direction);
                node.position = new Vector3f(move.Position);
                node.worldPosition = new Vector3f(move.InteractiveParam.Position);
                GetOwner().Send(new PacketScFactoryModifyChapterNodes(GetOwner(), chapterId, node));
                GetOwner().Send(new PacketScFactoryOpRet(GetOwner(), node.nodeId, op), seq);
                node.SendEntity(GetOwner(), chapterId);
            }
            else
            {
                ScFactoryOpRet ret = new()
                {
                    RetCode = FactoryOpRetCode.Fail,
                };
                GetOwner().Send(ScMsgId.ScFactoryOpRet, ret, seq);
            }
        }
        public void DismantleNode(CsFactoryOp op, ulong seq)
        {
            var dismantle = op.Dismantle;

            FactoryNode nodeRem = nodes.Find(n => n.nodeId == dismantle.NodeId);
            if (nodeRem != null)
            {
                RemoveConnectionsToNode(nodeRem, nodes);
                nodes.Remove(nodeRem);
                GetOwner().Send(ScMsgId.ScFactoryModifyChapterMap, new ScFactoryModifyChapterMap()
                {
                    ChapterId = chapterId,
                    MapId = nodeRem.mapId,
                    Tms = DateTime.UtcNow.ToUnixTimestampMilliseconds() / 1000,
                    Wires =
                    {
                        GetWires()
                    }
                });
                GetOwner().Send(new PacketScFactoryModifyChapterNodes(GetOwner(), chapterId, nodeRem.nodeId));
                GetOwner().Send(new PacketScFactoryOpRet(GetOwner(), nodeRem.nodeId, op), seq);
            }
            else
            {
                ScFactoryOpRet ret = new()
                {
                    RetCode = FactoryOpRetCode.Fail,

                };
                GetOwner().Send(ScMsgId.ScFactoryOpRet, ret, seq);
            }
        }
        public void RemoveConnectionsToNode(FactoryNode nodeRem, List<FactoryNode> allNodes)
        {
            // Ottieni tutti i compId del nodo da rimuovere
            HashSet<ulong> remCompIds = nodeRem.components.Select(c => (ulong)c.compId).ToHashSet();

            foreach (var node in allNodes)
            {
                node.connectedComps.RemoveAll(conn =>
                    remCompIds.Contains(conn.Key) || remCompIds.Contains(conn.Value));
            }
        }


        public uint nextCompV()
        {
            compV++;
            return compV;
        }
        public FactoryNode.FComponent GetCompById(ulong compId)
        {
            foreach(FactoryNode node in nodes)
            {
                if (node.components.Find(c => c.compId == compId) != null)
                {
                    return node.components.Find(c => c.compId == compId);
                }
            }
            return null;
        }
        public FactoryNode GetNodeByCompId(ulong compId)
        {
            foreach (FactoryNode node in nodes)
            {
                if (node.components.Find(c => c.compId == compId) != null)
                {
                    return node;
                }
            }
            return null;
        }
        private void AddConnection(CsFactoryOp op,ulong seq)
        {
            FactoryNode.FComponent nodeFrom = GetCompById(op.AddConnection.FromComId);
            FactoryNode.FComponent nodeTo = GetCompById(op.AddConnection.ToComId);

            if(nodeFrom!=null && nodeTo != null)
            {
                GetNodeByCompId(nodeFrom.compId).connectedComps.Add(new(nodeFrom.compId, nodeTo.compId));
                GetNodeByCompId(nodeTo.compId).connectedComps.Add(new(nodeTo.compId, nodeFrom.compId));
                GetOwner().Send(ScMsgId.ScFactoryModifyChapterMap, new ScFactoryModifyChapterMap()
                {
                    ChapterId = chapterId,
                    MapId = GetNodeByCompId(nodeFrom.compId).mapId,
                    Tms = DateTime.UtcNow.ToUnixTimestampMilliseconds()/1000,
                    Wires =
                    {
                        GetWires()
                    }
                });
                var wire = GetWires().Find(w =>
    (w.FromComId == op.AddConnection.FromComId && w.ToComId == op.AddConnection.ToComId) ||
    (w.FromComId == op.AddConnection.ToComId && w.ToComId == op.AddConnection.FromComId));

                if (wire != null)
                {
                    GetOwner().Send(new PacketScFactoryOpRet(GetOwner(), (uint)wire.Index, op), seq);
                }
                else
                {
                    // Facoltativo: log di errore
                    Console.WriteLine($"[WARN] Connessione non trovata tra {op.AddConnection.FromComId} e {op.AddConnection.ToComId}");
                }

            }

        }
        void ResetAllPower(List<FactoryNode> allNodes)
        {
            foreach (var node in allNodes)
                node.powered = false;
        }
        void UpdatePowerGrid(List<FactoryNode> allNodes)
        {
            ResetAllPower(allNodes);

            HashSet<uint> visited = new();

            foreach (var node in allNodes)
            {
                if (node.templateId.Contains("hub") || node.templateId == "power_diffuser_1")
                {
                    //if(node.forcePowerOn)
                    if(node.templateId== "power_diffuser_1")
                    {
                        //Check inside factory region

                    }
                    else
                    {
                        PropagatePowerFrom(node, visited);
                    }
                    
                }
            }
        }
        void PropagatePowerFrom(FactoryNode node, HashSet<uint> visited)
        {
            if (visited.Contains(node.nodeId))
                return;

            visited.Add(node.nodeId);
            node.powered = true;
            if(node.templateId == "power_diffuser_1")
            {
                //get builds in area test
                List<FactoryNode> nodes=GetNodesInRange(node.position, 15);
                foreach(FactoryNode propagateNode in nodes)
                {
                    if (propagateNode.GetComponent<FComponentPowerPole>() == null)
                    {
                        propagateNode.powered = true;
                    }
                }
            }
            if (node.GetComponent<FComponentPowerPole>() != null)
            foreach (var connectedCompId in node.connectedComps)
            {
                FactoryNode connectedNode = GetNodeByCompId(connectedCompId.Value);
                if (connectedNode != null)
                {
                    PropagatePowerFrom(connectedNode, visited);
                }
            }
        }
        private void CreateNode(CsFactoryOp op, ulong seq)
        {
            v++;
            uint nodeId = v;
            CsdFactoryOpPlace place = op.Place;
            FactoryBuildingTable table = ResourceManager.factoryBuildingTable[place.TemplateId];
            FactoryNode node = new()
            {
                nodeId = nodeId,
                templateId = place.TemplateId,
                mapId = place.MapId,
                sceneNumId = GetOwner().sceneManager.GetCurScene().sceneNumId,
                nodeType = table.GetNodeType(),
                position = new Vector3f(place.Position),
                direction = new Vector3f(place.Direction),
                worldPosition = new Vector3f(place.InteractiveParam.Position),
                guid = GetOwner().random.NextRand(),
                
            };
            
            node.InitComponents(this);
            GetOwner().Send(new PacketScFactoryModifyChapterNodes(GetOwner(), chapterId, node));
            nodes.Add(node);
            node.SendEntity(GetOwner(), chapterId);
           
            GetOwner().Send(new PacketScFactoryOpRet(GetOwner(), node.nodeId,op),seq);
            
        }

        public FactoryChapter(string chapterId,ulong ownerId)
        {
            this.ownerId = ownerId;
            this.chapterId = chapterId;
            FactoryNode node = new()
            {
                nodeId = v,
                templateId= "__inventory__",
                nodeType=FCNodeType.Inventory,
                mapId=0,
                deactive=true,
                guid = GetOwner().random.NextRand()
            };
            node.InitComponents(this);
            nodes.Add(node);
        }
        public Player GetOwner()
        {
            return Server.clients.Find(c => c.roleId == ownerId);
        }
    }
    public class FactoryNode
    {
        public uint nodeId;
        public FCNodeType nodeType;
        public string templateId;
        public Vector3f position=new();
        public Vector3f direction = new();
        public Vector3f worldPosition = new();
        public string instKey="";
        public bool deactive = false;
        public int mapId;
        public int sceneNumId;
        public bool forcePowerOn = false;
        public List<FComponent> components = new();
        [BsonIgnore]
        public bool powered = false;
        [BsonIgnore]
        public bool lastPowered = false;
        public List<ConnectedComp> connectedComps = new();
        public ulong guid;

        public class ConnectedComp
        {
            public ulong Key;
            public ulong Value;
            public ConnectedComp(ulong key, ulong value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
        public void Update(FactoryChapter chapter)
        {
            LevelScene scen = GetLevelData(sceneNumId);
            if (lastPowered != powered)
            {
                lastPowered = powered;
                chapter.GetOwner().Send(new PacketScFactoryModifyChapterNodes(chapter.GetOwner(), chapter.chapterId, this));
            }
            
        }
        public bool InPower()
        {
            if (forcePowerOn)
            {
                return true;
            }
            return powered;
        }
        public FComponent GetComponent<FComponent>() where FComponent : class
        {
            return components.Find(c => c is FComponent) as FComponent;
        }
        public FMesh GetMesh()
        {
            FMesh mesh = new FMesh();

            if (ResourceManager.factoryBuildingTable.TryGetValue(templateId, out FactoryBuildingTable table))
            {
                float width = table.range.width - 1;
                float height = table.range.height - 1;
                float depth = table.range.depth-1;

                Vector3f p1_final = new Vector3f();
                Vector3f p2_final = new Vector3f();

                switch (direction.y)
                {
                    case 0f:
                    case 360f:
                    default:
                        p1_final = position + new Vector3f(table.range.x, table.range.y, table.range.z);
                        p2_final = p1_final + new Vector3f(width, height, depth);
                        break;

                    case 90f:
                        // Rotazione 90°: Larghezza e profondità si scambiano.
                        // Il mesh parte da 'position' ma si estende su assi diversi.
                        p1_final = position + new Vector3f(table.range.x, table.range.y, table.range.z - width);
                        p2_final = p1_final + new Vector3f(depth, height, width);
                        break;

                    case 180f:
                        // Rotazione 180°: Larghezza e profondità mantengono i loro valori
                        // ma il mesh si estende in direzioni negative.
                        p1_final = position + new Vector3f(table.range.x - width, table.range.y, table.range.z - depth);
                        p2_final = p1_final + new Vector3f(width, height, depth);
                        break;

                    case 270f:
                        // Rotazione 270°: Larghezza e profondità si scambiano.
                        // Il mesh si estende in direzioni diverse rispetto alla rotazione di 90°.
                        p1_final = position + new Vector3f(table.range.x - depth, table.range.y, table.range.z);
                        p2_final = p1_final + new Vector3f(depth, height, width);
                        break;
                }

                mesh.points.Add(p1_final);
                mesh.points.Add(p2_final);
            }

            return mesh;
        }


        public ScdFacNode ToProto()
        {
            ScdFacNode node = new ScdFacNode()
            {
                InstKey = instKey,
                NodeId=nodeId,
                TemplateId=templateId,
                StableId= GetStableId(),
                IsDeactive= deactive,
                
                Power = new()
                {
                    InPower= InPower(),
                    NeedInPower=true,
                },
                
                NodeType=(int)nodeType,
                Transform = new()
                {
                    Position = position.ToProtoScd(),
                    Direction=direction.ToProtoScd(),
                    MapId=mapId,
                    
                    
                }
            };
            if(templateId!="__inventory__")
            {
                node.Transform.Mesh = GetMesh().ToProto();
                node.Transform.Position = position.ToProtoScd();
                node.Transform.WorldPosition = worldPosition.ToProto();
                node.Transform.WorldRotation = direction.ToProto();
                node.InteractiveObject = new()
                {
                   ObjectId=guid,
                };
                node.Flag = 0;
                node.InstKey = "";
            }
            foreach(FComponent comp in components)
            {
                node.Components.Add(comp.ToProto());
                node.ComponentPos.Add((int)comp.GetComPos(), comp.compId);
            }

            return node;
        }
        public uint GetStableId()
        {
            return 10000+nodeId;
        }
        public FCComponentType GetMainCompType()
        {
            string nodeTypeName = nodeType.ToString();
            if (Enum.TryParse(nodeTypeName, out FCComponentType fromName))
            {
                return fromName;
            }
            return FCComponentType.Invalid;
        }
        public void InitComponents(FactoryChapter chapter)
        {
            switch (nodeType)
            {
                case FCNodeType.PowerPole:
                    components.Add(new FComponentPowerPole(chapter.nextCompV()).Init());
                    break;
                case FCNodeType.PowerDiffuser:
                    components.Add(new FComponentPowerPole(chapter.nextCompV()).Init());
                    break;
                case FCNodeType.Battle:
                    components.Add(new FComponentBattle(chapter.nextCompV()).Init());
                    break;
                case FCNodeType.Producer:

                    components.Add(new FComponentPortManager(chapter.nextCompV(), 3).Init());
                    components.Add(new FComponentPortManager(chapter.nextCompV(), 3).Init());
                    components.Add(new FComponentProducer(chapter.nextCompV()).Init());
                    components.Add(new FComponentFormulaMan(chapter.nextCompV()).Init());
                    components.Add(new FComponentCache(chapter.nextCompV()).Init());
                    components.Add(new FComponentCache(chapter.nextCompV()).Init());
                    components.Add(new FComponentCache(chapter.nextCompV()).Init());
                    break;
                case FCNodeType.TravelPole:
                    components.Add(new FComponentTravelPole(chapter.nextCompV()).Init());
                    break;
                case FCNodeType.Hub:
                    components.Add(new FComponentSelector(chapter.nextCompV()).Init());
                    components.Add(new FComponentPowerPole(chapter.nextCompV()).Init());
                    components.Add(new FComponentPowerSave(chapter.nextCompV()).Init());
                    components.Add(new FComponentStablePower(chapter.nextCompV()).Init());
                    components.Add(new FComponentBusLoader(chapter.nextCompV()).Init());    
                    components.Add(new FComponentPortManager(chapter.nextCompV(),GetComponent<FComponentBusLoader>().compId).Init());
                    forcePowerOn = true;
                    break;
                case FCNodeType.SubHub:
                    components.Add(new FComponentSubHub(chapter.nextCompV()).Init());
                    components.Add(new FComponentSelector(chapter.nextCompV()).Init());
                    components.Add(new FComponentPowerPole(chapter.nextCompV()).Init());
                    components.Add(new FComponentPowerSave(chapter.nextCompV()).Init());
                    components.Add(new FComponentStablePower(chapter.nextCompV()).Init());
                    components.Add(new FComponentBusLoader(chapter.nextCompV()).Init());
                    components.Add(new FComponentPortManager(chapter.nextCompV(), GetComponent<FComponentBusLoader>().compId).Init());
                    forcePowerOn = true;
                    break;
                default:
                    components.Add(new FComponent(chapter.nextCompV(), GetMainCompType()).Init());
                    break;
            }

        }

        public void SendEntity(Player player, string chapterId)
        {
            Entity exist = player.sceneManager.GetCurScene().entities.Find(e => e.guid == guid);
            if (exist != null)
            {
                exist.Position = worldPosition;
                exist.Rotation = direction;
                ScMoveObjectMove move = new()
                {
                    ServerNotify = true,
                    MoveInfo =
                    {
                        new MoveObjectMoveInfo()
                        {
                            Objid = guid,
                            SceneNumId=sceneNumId,
                            MotionInfo = new()
                            {
                                Position=exist.Position.ToProto(),
                                Rotation=exist.Rotation.ToProto(),
                                Speed=new Vector()
                                {
                                    
                                },
                                State=MotionState.MotionNone
                            }
                        }
                    }
                };
                player.Send(ScMsgId.ScMoveObjectMove, move);
            }
            else
            {
                if (interactiveFacWrapperTable.ContainsKey(templateId))
                {
                    EntityInteractive e = new(interactiveFacWrapperTable[templateId].interactiveTemplateId, player.roleId, worldPosition, direction, sceneNumId, guid);
                    e.InitDefaultProperties();
                    e.SetPropValue(nodeId, "factory_inst_id");

                    player.sceneManager.GetCurScene().entities.Add(e);
                    player.sceneManager.GetCurScene().SpawnEntity(e);
                }
                
            }
           
        }

        [BsonDiscriminator(Required = true)]
        [BsonKnownTypes(typeof(FComponentSelector))]

        public class FComponent
        {
            public class FCompInventory
            {
                public ScdFacComInventory ToProto()
                {
                    return new ScdFacComInventory()
                    {

                    };
                }
            }
            public uint compId;
            public FCComponentType type;
            public FCompInventory inventory;
            
            public FComponent(uint id, FCComponentType t)
            {
                this.compId = id;
                this.type = t;
            }
            public FCComponentPos GetComPos()
            {

                string compTypeName = type.ToString();
                if (Enum.TryParse(compTypeName, out FCComponentPos fromName))
                {
                    return fromName;
                }
                switch (type)
                {
                    case FCComponentType.PowerPole:
                        return FCComponentPos.PowerPole;
                    case FCComponentType.TravelPole:
                        return FCComponentPos.TravelPole;
                    case FCComponentType.Battle:
                        return FCComponentPos.Battle1;
                    case FCComponentType.Producer:
                        return FCComponentPos.Producer;
                    case FCComponentType.FormulaMan:
                        return FCComponentPos.FormulaMan;
                }
                return FCComponentPos.Invalid;
            }
            public ScdFacCom ToProto()
            {
                ScdFacCom proto = new ScdFacCom()
                {
                    ComponentType = (int)type,
                    ComponentId = compId,

                };
                SetComponentInfo(proto);
                return proto;
            }

            public virtual void SetComponentInfo(ScdFacCom proto)
            {
                if (inventory != null)
                {
                    proto.Inventory = inventory.ToProto();
                }
                else if (type == FCComponentType.PowerPole)
                {
                    proto.PowerPole = new()
                    {

                    };
                }
            }

            public virtual FComponent Init()
            {
                switch (type)
                {
                    case FCComponentType.Inventory:
                        inventory = new();
                        break;
                    default:
                        break;
                }
                return this;    
            }
        }
        public class FMesh
        {
            public FCMeshType type;
            public List<Vector3f> points=new();
            public ScdFacMesh ToProto()
            {
                ScdFacMesh m = new ScdFacMesh()
                {
                    MeshType = (int)type
                };
                foreach (Vector3f p in points)
                {
                    m.Points.Add(new ScdVec3Int()
                    {
                        X = (int)p.x,
                        Y = (int)p.y,
                        Z = (int)p.z
                    });
                }
                return m;
            }
        }
    }

}
