using System;
using TableIT.Core;

namespace TableIT.Remote
{
    public class TableClientManager
    {
        private string? UserId { get; set; }
        private TableClient? Client { get; set; }

        public TableClient GetClient() => Client ??= new TableClient(userId: UserId);

        internal void WithUserId(string userId) => UserId = userId;
    }
}
