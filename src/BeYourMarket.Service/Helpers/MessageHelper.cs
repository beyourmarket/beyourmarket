using BeYourMarket.Core;
using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
using BeYourMarket.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace BeYourMarket.Service
{
    public static class MessageHelper
    {
        public static MessageReadStateService MessageReadStateService
        {
            get
            {
                return ContainerManager.GetConfiguredContainer().Resolve<BeYourMarket.Service.MessageReadStateService>();
            }
        }

        public static MessageService MessageService
        {
            get
            {
                return ContainerManager.GetConfiguredContainer().Resolve<BeYourMarket.Service.MessageService>();
            }
        }

        /// <summary>
        /// Get UnRead Messages
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<Message> GetUnReadMessages(string userId)
        {
            var messageReadStates = MessageReadStateService
                .Query(x => x.UserID == userId && !x.ReadDate.HasValue)
                .Include(x => x.Message)
                .Include(x => x.Message.AspNetUser)
                .Select();

            var messages = messageReadStates
                .OrderByDescending(x => x.Created)
                .Select(x => x.Message)                
                .ToList();

            return messages;
        }
    }
}
