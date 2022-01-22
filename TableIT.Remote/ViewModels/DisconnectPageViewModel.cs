using Microsoft.Toolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using TableIT.Remote.Messages;

namespace TableIT.Remote.ViewModels
{
    public class DisconnectPageViewModel
    {
        public TableClientManager ClientManager { get; }
        private IMessenger Messenger { get; }
        
        public DisconnectPageViewModel(TableClientManager clientManager, IMessenger messenger)
        {
            ClientManager = clientManager ?? throw new System.ArgumentNullException(nameof(clientManager));
            Messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
        }

        public async Task Disconnect()
        {
            await ClientManager.Disconnect();
            Messenger.Send(new TableDisconnected());
        }
    }
}
