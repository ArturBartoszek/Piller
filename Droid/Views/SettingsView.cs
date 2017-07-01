using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Piller.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using MvvmCross.Binding.BindingContext;
using Android.Media;

namespace Piller.Droid.Views
{
    [Activity]
    public class SettingsView : MvxAppCompatActivity<SettingsViewModel>
    {
        RelativeLayout morning;
        RelativeLayout afternoon;
        RelativeLayout evening;
        TextView morningHour;
        TextView afternoonHour;
        TextView eveningHour;
        RadioButton ringBtn;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Settings);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.Title = "Ustawienia";

            morning = FindViewById<RelativeLayout>(Resource.Id.morning);
            afternoon = FindViewById<RelativeLayout>(Resource.Id.afternoon);
            evening = FindViewById<RelativeLayout>(Resource.Id.evening);

            morningHour = FindViewById<TextView>(Resource.Id.morningHour);
            afternoonHour = FindViewById<TextView>(Resource.Id.afternoonHour);
            eveningHour = FindViewById<TextView>(Resource.Id.eveningHour);

            ringBtn = FindViewById<RadioButton>(Resource.Id.ringBtn);
            SetBinding();

            morning.Click += (o, e) =>
            {
                TimePickerDialog timePicker = new TimePickerDialog(
                    this,
                    (s, args) =>
                    {
                        if (((TimePicker)s).IsShown)
                            this.ViewModel.SetMorning.Execute(new TimeSpan(args.HourOfDay, args.Minute, 0)).Subscribe();
                    },
                     12,
                     00,
                     true);
                timePicker.Show();
            };
            afternoon.Click += (o, e) =>
            {
                TimePickerDialog timePicker = new TimePickerDialog(
                    this,
                    (s, args) =>
                    {
                        if (((TimePicker)s).IsShown)
                            this.ViewModel.SetAfternoon.Execute(new TimeSpan(args.HourOfDay, args.Minute, 0)).Subscribe();
                    },
                     12,
                     00,
                     true);
                timePicker.Show();
            };
            evening.Click += (o, e) =>
            {
                TimePickerDialog timePicker = new TimePickerDialog(
                    this,
                    (s, args) =>
                    {
                        if (((TimePicker)s).IsShown)
                            this.ViewModel.SetEvening.Execute(new TimeSpan(args.HourOfDay, args.Minute, 0)).Subscribe();
                    },
                     12,
                     00,
                     true);
                timePicker.Show();
            };

            ringBtn.Click += (o, e) =>
            {
                Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
                intent.PutExtra(RingtoneManager.ExtraRingtoneTitle, true);
                intent.PutExtra(RingtoneManager.ExtraRingtoneShowSilent, false);
                intent.PutExtra(RingtoneManager.ExtraRingtoneShowDefault, true);
                intent.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, RingtoneManager.GetDefaultUri(RingtoneType.Alarm));
                StartActivityForResult(Intent.CreateChooser(intent, "Wybierz dzwonek"), 0);

                //Android.Net.Uri ring = (Android.Net.Uri)intent.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
            };
        }

        private void SetBinding()
        {
            var bindingSet = this.CreateBindingSet<SettingsView, SettingsViewModel>();
            bindingSet.Bind(morningHour)
                .For(v => v.Text)
                .To(vm => vm.MorningHour)
                .WithConversion(new InlineValueConverter<TimeSpan, string>(t => $"{t:hh\\:mm}"));
            bindingSet.Bind(afternoonHour)
               .To(vm => vm.AfternoonHour)
                .WithConversion(new InlineValueConverter<TimeSpan, string>(t => $"{t:hh\\:mm}"));
            bindingSet.Bind(eveningHour)
               .To(vm => vm.EveningHour)
                .WithConversion(new InlineValueConverter<TimeSpan, string>(t => $"{t:hh\\:mm}"));
            bindingSet.Apply();

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                Android.Net.Uri ring = (Android.Net.Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                Ringtone ringtone = RingtoneManager.GetRingtone(this, ring);
                String title = ringtone.GetTitle(this);
                if (title.Contains("Default ringtone (Flutey Phone)"))
                    ringBtn.Text = "Default";
                else
                    ringBtn.Text = title;

                this.ViewModel.SetRingUri.Execute(ring.ToString()).Subscribe();
            }
        }

        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.settings_menu,menu);
            return base.OnCreateOptionsMenu(menu);
        }
        
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int goHomeId = 16908332;
            if (item.ItemId == Resource.Id.action_save)
                ViewModel.Save.Execute().Subscribe();
            else if (item.ItemId == goHomeId)              
                OnBackPressed();
            return true;
        }
        


    }
}