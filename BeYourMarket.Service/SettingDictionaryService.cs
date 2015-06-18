using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public interface ISettingDictionaryService : IService<SettingDictionary>
    {
        void SaveSettingDictionary(SettingDictionary setting);
        Task<SettingDictionary> GetSettingDictionary(int settingID, Enum_SettingKey settingKey);
    }

    public class SettingDictionaryService : Service<SettingDictionary>, ISettingDictionaryService
    {
        public SettingDictionaryService(IRepositoryAsync<SettingDictionary> repository)
            : base(repository)
        {
        }

        public void SaveSettingDictionary(SettingDictionary setting)
        {
            if (setting.ID == 0)
            {
                setting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added;
                Insert(setting);
            }
            else
            {
                setting.ObjectState = Repository.Pattern.Infrastructure.ObjectState.Modified;
                Update(setting);
            }
        }

        public async Task<SettingDictionary> GetSettingDictionary(int settingID, Enum_SettingKey settingKey)
        {
            var settingQuery = await Query(x => x.Name == settingKey.ToString() && x.ID == settingID).SelectAsync();
            var setting = settingQuery.FirstOrDefault();

            if (setting == null)
                return new SettingDictionary()
                {
                    Name = settingKey.ToString(),
                    Value = string.Empty
                };

            return setting;
        }
    }
}
