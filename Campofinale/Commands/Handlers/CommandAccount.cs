using Campofinale.Database;
using Campofinale.Protocol;
using Campofinale.Resource;

namespace Campofinale.Commands.Handlers
{
    public static class CommandAccount
    {
        [Server.Command("account", "account command")]
        public static void Handle(Player sender,string cmd, string[] args, Player target)
        {
            if (sender != null)
            {
                CommandManager.SendMessage(sender, "This command can't be used ingame");
                return;
            }
            if (args.Length < 2)
            {
                CommandManager.SendMessage(sender, "Usage: account create|reset|delete (username)");
                return;
            }
            switch (args[0])
            {
                case "create":
                    DatabaseManager.db.CreateAccount(args[1]);
                    break;
                case "reset":
                    var account = DatabaseManager.db.GetAccountByUsername(args[1]);
                    if (account == null)
                    {
                        CommandManager.SendMessage(sender, $"Account with username {args[1]} not found");
                        return;
                    }

                    Player online = Server.clients.Find(c => c.accountId == account.id);
                    if (online != null)
                    {
                        online.Kick(CODE.ErrServerClosed, "Account reset");
                    }

                    var result = DatabaseManager.db.ResetAccount(account.username);
                    CommandManager.SendMessage(sender, result.Item1);
                    break;
                case "delete":
                    var accountDelete = DatabaseManager.db.GetAccountByUsername(args[1]);
                    if (accountDelete == null)
                    {
                        CommandManager.SendMessage(sender, $"Account with username {args[1]} not found");
                        return;
                    }

                    Player onlineDelete = Server.clients.Find(c => c.accountId == accountDelete.id);
                    if (onlineDelete != null)
                    {
                        onlineDelete.Kick(CODE.ErrServerClosed, "Account deleted");
                    }

                    var deleteResult = DatabaseManager.db.DeleteAccount(accountDelete.username);
                    CommandManager.SendMessage(sender, deleteResult.Item1);
                    break;
                default:
                    CommandManager.SendMessage(sender, "Example: account create (username)");
                    break;
            }
        }
    }
}
