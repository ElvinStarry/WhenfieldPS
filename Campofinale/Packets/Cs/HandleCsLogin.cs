using BeyondTools.VFS.Crypto;
using Campofinale.Database;
using Campofinale.Game;
using Campofinale.Network;
using Campofinale.Packets.Sc;
using Campofinale.Protocol;
using Campofinale.Resource;
using System.Security.Cryptography;
using static Campofinale.Resource.ResourceManager;

namespace Campofinale.Packets.Cs
{
    public class HandleCsLogin
    {
        [Server.Handler(CsMsgId.CsSetGender)]
        public static void HandleCsSetGender(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSetGender req = packet.DecodeBody<CsSetGender>();
            ScSetGender rsp = new()
            {
                Gender = req.Gender,
            };
            session.gender = rsp.Gender;
            session.Send(ScMsgId.ScSetGender, rsp);
            
        }
        [Server.Handler(CsMsgId.CsLogin)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsLogin req = packet.DecodeBody<CsLogin>();
            if(Server.clients.Count > Server.config.serverOptions.maxPlayers)
            {
                session.Send(ScMsgId.ScNtfErrorCode, new ScNtfErrorCode()
                {
                    Details = "Server Full",
                    ErrorCode = (int)CODE.ErrCommonServerOverload,
                });
                session.Disconnect();
                return;
            }
            Account account = DatabaseManager.db.GetAccountByTokenGrant(req.Token);
            ScLogin rsp = new()
            {
                IsEnc = false,
                Uid = req.Uid,
                IsFirstLogin = false,
                IsReconnect=false,
                LastRecvUpSeqid = packet.csHead.UpSeqid,
                
            };
            byte[] encKey = GenerateRandomBytes(32);
            string serverPublicKeyPem = req.ClientPublicKey.ToStringUtf8();
            byte[] serverPublicKey = ConvertPemToBytes(serverPublicKeyPem);
            byte[] encryptedEncKey = EncryptWithRsa(encKey, serverPublicKey);
            byte[] serverEncrypNonce = GenerateRandomBytes(12);
           // rsp.ServerEncrypNonce = ByteString.CopyFrom(serverEncrypNonce);
           // rsp.ServerPublicKey = ByteString.CopyFrom(encryptedEncKey);
       
            CSChaCha20 cipher = new CSChaCha20(encKey, serverEncrypNonce, 1);
            if (req.ClientVersion == GameConstants.GAME_VERSION)
            {
                if (account == null)
                {
                    session.Send(ScMsgId.ScNtfErrorCode, new ScNtfErrorCode()
                    {
                        Details = "Account error",
                        ErrorCode = (int)CODE.ErrLoginProcessLogin,
                    });
                    session.Disconnect();
                    return;
                }
                bool exist=session.Load(account.id);
                
                rsp.Uid = ""+session.accountId;
                if (!exist)
                {
                    rsp.IsFirstLogin = true;
                    //session.gender = Gender.GenInvalid;

                    //session.Send(ScMsgId.ScLogin, rsp);
                    //session.Send(new PacketScSyncBaseData(session));
                    //return;
                }
                session.Send(ScMsgId.ScLogin, rsp);
                
            }
            else
            {
                session.Send(ScMsgId.ScNtfErrorCode, new ScNtfErrorCode()
                {
                    Details="Unsupported client version",
                    ErrorCode= (int)CODE.ErrCommonClientVersionNotEqual
                });
                session.Disconnect();
                return;
            }
            session.Send(new PacketScSyncBaseData(session));
            ScItemBagCommonSync common = new()
            {
                LostAndFound = new()
                {
                    InstList =
                    {
                        new ScdItemGrid()
                        {
                            GridIndex=0,
                            Count=1,
                            Id="item_port_power_pole_2",
                            Inst = new()
                            {
                                InstId=300000000000,
                                
                            },
                            
                        }
                    }
                },
                
            };
            session.Send(ScMsgId.ScItemBagCommonSync, common);
            session.Send(new PacketScItemBagScopeSync(session, ItemValuableDepotType.Weapon));
            session.Send(new PacketScItemBagScopeSync(session, ItemValuableDepotType.WeaponGem));
            session.Send(new PacketScItemBagScopeSync(session, ItemValuableDepotType.Equip));
            session.Send(new PacketScItemBagScopeSync(session, ItemValuableDepotType.CommercialItem));
            session.Send(new PacketScItemBagScopeSync(session, ItemValuableDepotType.Factory));
            session.Send(new PacketScItemBagScopeSync(session, ItemValuableDepotType.SpecialItem));
            session.Send(new PacketScSyncAllMail(session));
            session.Send(new PacketScSceneCollectionSync(session));
            string json1 = File.ReadAllText("44_ScSyncAllMission.json");
           
            
            ScSyncAllMission m = Newtonsoft.Json.JsonConvert.DeserializeObject<ScSyncAllMission>(json1);
            m.TrackMissionId = "";
            
            
            session.Send(ScMsgId.ScSyncAllMission, m);
            /*ession.Send(ScMsgId.ScSyncAllMission, new ScSyncAllMission()
            {
                NewMissionTags =
                {
                    
                },
                
                Missions =
                {
                    {"e0m0", new Mission()
                    {
                        MissionId="e0m0",
                        MissionState=(int)MissionState.Processing,
                        SucceedId=-1,
                        Properties =
                        {
                            {1,new DynamicParameter()
                            {
                                RealType=1,
                                ValueType=1,
                                ValueBoolList =
                                {
                                    false
                                }
                            } }
                        }
                    } }
                },
                TrackMissionId = "e0m0",
                
                CurQuests =
                {
                    
                    {"e0m0_q#1", new Quest()
                    {
                        QuestId="e0m0_q#1",
                        QuestState=(int)QuestState.Processing,
                        QuestObjectives =
                        {
                            new QuestObjective()
                            {
                                ConditionId="f6415b84",
                                
                                IsComplete=false
                            }
                        }
                    } },
                    {"e0m0_q#2", new Quest()
                    {
                        QuestId="e0m0_q#2",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {
                           new QuestObjective()
                           {
                               ConditionId="81736ca7",
                               IsComplete=false,
                           }
                        }
                    } },
                    {"e0m0_q#3", new Quest()
                    {
                        QuestId="e0m0_q#3",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {
 
                        }
                    } },
                    {"e0m0_q#4", new Quest()
                    {
                        QuestId="e0m0_q#4",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {

                        }
                    } },
                    {"e0m0_q#5", new Quest()
                    {
                        QuestId="e0m0_q#5",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {

                        }
                    } },
                    {"e0m0_q#6", new Quest()
                    {
                        QuestId="e0m0_q#6",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {

                        }
                    } },
                    {"e0m0_q#7", new Quest()
                    {
                        QuestId="e0m0_q#7",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {

                        }
                    } },
                    {"e0m0_q#8", new Quest()
                    {
                        QuestId="e0m0_q#8",
                        QuestState=(int)QuestState.Available,
                        QuestObjectives =
                        {

                        }
                    } }
                }
            });*/
            
            session.Send(new PacketScGachaSync(session));
            ScSettlementSyncAll settlements = new ScSettlementSyncAll()
            {
                LastTickTime = DateTime.UtcNow.ToUnixTimestampMilliseconds(),
                
            };
            int stid = 3;
            foreach (var item in settlementBasicDataTable)
            {
                settlements.Settlements.Add(new Settlement()
                {
                    Level = 1,
                    SettlementId = item.Value.settlementId,
                    RequireId = "item_plant_grass_powder_2",
                    Exp = 1,
                    Reports =
                    {

                    },
                    UnlockTs = DateTime.UtcNow.AddHours(1).ToUnixTimestampMilliseconds(),
                    AutoSubmit = false,
                    LastManualSubmitTime = DateTime.UtcNow.ToUnixTimestampMilliseconds(),
                    
                    OfficerCharTemplateId = characterTable.Values.ToList()[stid].charId,

                    
                });
                stid++;
            }
            
            session.Send(ScMsgId.ScSettlementSyncAll, settlements);
            session.Send(new PacketScSyncAllRoleScene(session));
            session.Send(new PacketScGameMechanicsSync(session));
            session.Send(new PacketScSyncAllBloc(session));
            session.Send(new PacketScSyncWallet(session));
            session.Send(new PacketScSyncAllGameVar(session));
            session.Send(new PacketScSyncAllUnlock(session));
            session.Send(new PacketScSyncAllBitset(session));
            session.Send(new PacketScSyncAllMiniGame(session));
           
            string json = File.ReadAllText("93_ScSceneMapMarkSync.json");
            ScSceneMapMarkSync chapter = Newtonsoft.Json.JsonConvert.DeserializeObject<ScSceneMapMarkSync>(json);
            session.Send(ScMsgId.ScSceneMapMarkSync, chapter);
            session.Send(new PacketScAdventureBookSync(session));
            session.Send(new PacketScAdventureSyncAll(session));
            session.Send(new PacketScFactorySync(session));
            session.Send(new PacketScFactorySyncScope(session));
            session.Send(new PacketScFactorySyncChapter(session, "domain_1"));
            session.Send(new PacketScFactorySyncChapter(session, "domain_2"));
            session.Send(new PacketScSyncCharBagInfo(session));
            session.Send(new PacketScSyncAllDialog(session));
            session.Send(new PacketScSpaceshipSync(session));
            session.Send(new PacketScSyncFullDungeonStatus(session));
            session.Send(new PacketScActivitySync(session));
            session.Send(new PacketScSnsGetChatList(session));
            session.Send(ScMsgId.ScSyncFullDataEnd, new ScSyncFullDataEnd());
            session.EnterScene();
            session.Initialized = true;
            session.Update();
            
            
        }
        static byte[] GenerateRandomBytes(int length)
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return bytes;
        }
        static byte[] ConvertPemToBytes(string pem)
        {
            string base64Key = pem
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "");
            return Convert.FromBase64String(base64Key);
        }

        // Crittografare con RSA (PKCS#1)
        static byte[] EncryptWithRsa(byte[] data, byte[] publicKeyBytes)
        {
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

    }
}
