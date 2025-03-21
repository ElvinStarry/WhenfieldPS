using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    public class HandleCsSceneTeleport
    {

        [Server.Handler(CsMsgId.CsSceneTeleport)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneTeleport req = packet.DecodeBody<CsSceneTeleport>();
            
            if (session.curSceneNumId != req.SceneNumId)
            {
                session.EnterScene(req.SceneNumId, new Resource.ResourceManager.Vector3f(req.Position), new Resource.ResourceManager.Vector3f(req.Rotation));
            }
            else
            {
                ScSceneTeleport t = new()
                {
                    TeleportReason = req.TeleportReason,
                    PassThroughData = req.PassThroughData,
                    Position = req.Position,
                    Rotation = req.Rotation,
                    SceneNumId = req.SceneNumId,
                };
                session.curSceneNumId = t.SceneNumId;
                session.Send(ScMsgId.ScSceneTeleport, t);
            }
            
            

        }
       
    }
}
