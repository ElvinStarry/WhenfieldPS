using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    public class HandleCsSceneSetLastSafeZone
    {

        [Server.Handler(CsMsgId.CsSceneSetLastSafeZone)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneSetLastSafeZone req = packet.DecodeBody<CsSceneSetLastSafeZone>();
            
            //TODO understand how to work
            if (req.SceneNumId != session.curSceneNumId)
            {
                //session.sceneManager.UnloadCurrent(true);
                session.curSceneNumId = req.SceneNumId;
                Logger.Print("Cur Scene id changed by SetLastSafeZone");

                //session.sceneManager.LoadCurrent();
                //session.EnterScene(req.SceneNumId,new Vector3f(req.Position),new Vector3f(req.Rotation));
            }
           
        }
       
    }
}
