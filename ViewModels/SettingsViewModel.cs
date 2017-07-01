﻿using Cheesebaron.MvxPlugins.Settings.Interfaces;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Piller.Data;
using ReactiveUI;
using System.Reactive;
using Newtonsoft.Json;
using System;
using System.Reactive.Linq;
using MvvmCross.Plugins.Messenger;
using System.Threading.Tasks;
using Piller.Services;
using System.Collections.Generic;

namespace Piller.ViewModels
{
    public class SettingsViewModel : MvxViewModel
    {
        private TimeSpan morningHour;
        public TimeSpan MorningHour
        {
            get { return morningHour; }
            set { SetProperty(ref morningHour, value); }
        }
        private TimeSpan afternoonHour;
        public TimeSpan AfternoonHour
        {
            get { return afternoonHour; }
            set { SetProperty(ref afternoonHour, value); }
        }
        private TimeSpan eveningHour;
        public TimeSpan EveningHour
        {
            get { return eveningHour; }
            set { SetProperty(ref eveningHour, value); }
        }
        private string ringUri;
        public string RingUri
        {
            get { return ringUri; }
            set { SetProperty(ref ringUri, value); }
        }
        public ReactiveCommand<TimeSpan, Unit> SetMorning { get; }
        public ReactiveCommand<TimeSpan, Unit> SetAfternoon { get; }
        public ReactiveCommand<TimeSpan, Unit> SetEvening { get; }
        public ReactiveCommand<String, Unit> SetRingUri { get; }
        public ReactiveCommand<Unit, bool> Save { get; }
        private SettingsData settingsData;
        private ISettings settings = Mvx.Resolve<ISettings>();
        private readonly string key = SettingsData.Key;

        public SettingsViewModel()
        {
            readSettings();
            MorningHour = settingsData.Morning;
            eveningHour = settingsData.Evening;

            RingUri = settingsData.RingUri;

            SetMorning = ReactiveCommand.Create<TimeSpan>(hour => MorningHour = hour);
            SetEvening = ReactiveCommand.Create<TimeSpan>(hour => EveningHour = hour);

            SetRingUri = ReactiveCommand.Create<String>(uri => RingUri = uri);

            Save = ReactiveCommand.Create(() =>
            {
                var data = JsonConvert.SerializeObject(new SettingsData() { Morning = this.MorningHour, Afternoon = this.AfternoonHour, Evening = this.EveningHour, RingUri = this.RingUri });
                settings.AddOrUpdateValue<string>(key, data);
                return true;
            });
            Save.Subscribe(async x =>
            {
                if (x)
                {
                    await reloadDataBase();
                    Mvx.Resolve<IMvxMessenger>().Publish(new SettingsChangeMessage(this,MorningHour,EveningHour, ringUri));
                    Close(this);
                }
            });
        }
        public async Task reloadDataBase()
        {
            IPermanentStorageService storage = Mvx.Resolve<IPermanentStorageService>();
            INotificationService notifications = Mvx.Resolve<INotificationService>();
            var items = await storage.List<MedicationDosage>();
            if (items == null)
                return;
            var medicationList = new List<MedicationDosage>(items);
            foreach(var item in medicationList)
            {
                var dosageHours = new List<TimeSpan>();
                if (item.Morning)
                    dosageHours.Add(MorningHour);
                if (item.Evening)
                    dosageHours.Add(EveningHour);
                item.DosageHours = dosageHours;
                item.RingUri = ringUri;
                await storage.SaveAsync<MedicationDosage>(item);
                await notifications.ScheduleNotification(item);
            }
        }



        private void readSettings()
        {
            var data = settings.GetValue<string>(key);
            if (String.IsNullOrEmpty(data))
            {
                settingsData = new SettingsData();
                return;
            }
            try
            {
                settingsData = JsonConvert.DeserializeObject<SettingsData>(data);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                settingsData = new SettingsData();
            }

        }
    }
}