using Campofinale.Game;
using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Sc
{
    public class PacketScFriendListSimpleSync : Packet
    {

        public PacketScFriendListSimpleSync(Player client) {

           
            ScFriendListSimpleSync proto = new ScFriendListSimpleSync()
            {
                
                FriendList =
                {
                    new ScdFriendFriendSimpleInfo()
                    {
                        AdventureLevel=1,
                        Name=client.nickname,
                        Online=true,
                        RoleId=(ulong)client.roleId,
                        Signature="ayo",
                        RemarkName=client.nickname,
                        UserAvatarFrameId=3,
                        UserAvatarId=8,
                        ShortId="1",
                        ThirdAccountData = new()
                        {
                            ThirdAccountDataType=HgThirdAccountType.AccountTypeDefault
                        },
                        LastLoginType=HgThirdAccountType.AccountTypeDefault
                    }
                },
                
                
            };
           
            SetData(ScMsgId.ScFriendListSimpleSync, proto);
        }

    }
}
