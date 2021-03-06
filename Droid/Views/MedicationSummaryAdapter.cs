﻿using System;
using Android.Runtime;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.Views;
using Piller.Data;
using MvvmCross.Binding.Droid.BindingContext;
using Android.Content;
using Piller.Droid.BindingConverters;
using Android.Views;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Plugins.File;
using MvvmCross.Platform;
using Android.Graphics;
using Services;

namespace Piller.Droid.Views
{
	public class MedicationSummaryAdapter : MvxAdapter
	{
        private readonly IMvxFileStore fileStore = Mvx.Resolve<IMvxFileStore>();
		private readonly ImageLoaderService imageLoader = Mvx.Resolve<ImageLoaderService>();

		public MedicationSummaryAdapter(Context context) : base(context)
        {
		}

		public MedicationSummaryAdapter(Context context, IMvxAndroidBindingContext bindingContext) : base(context, bindingContext)
        {
		}

		public MedicationSummaryAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
		}

        protected override IMvxListItemView CreateBindableView(object dataContext, int templateId)
        {
            var view = base.CreateBindableView(dataContext, templateId) as MvxListItemView;

            var name = view.FindViewById<TextView>(Resource.Id.label_medication_name);
			var date1 = view.FindViewById<TextView>(Resource.Id.label_medication_date);
			var date2 = view.FindViewById<TextView>(Resource.Id.label_medication_date2);
            var time = view.FindViewById<TextView>(Resource.Id.label_medication_time);
            var daysOfWeek = view.FindViewById<TextView>(Resource.Id.label_medication_days_of_week);

            var bset = view.CreateBindingSet<MvxListItemView, MedicationDosage>();

            bset.Bind(name)
                .To(x => x.Name);

			// Konwertery to specyficzny dla MvvmCross'a sposób translacji danych z view modelu do danych z których potrafi skorzystać widok.
			// Zazwyczaj nie są one potrzebne, np. kiedy pokazujemy tekst, ale jeśli zachodzi potrzeba pokazania np. listy w jednej linii musimy użyć konwertera.

			// TextView jest domyślnie bindowane do property Text, więc nie trzeba jej wprost wskazywać 
			bset.Bind(date1)
			    .To(x => x.From);
			bset.Bind(date2)
			    .To(x => x.To);
			
            bset.Bind(time)
                .To(x => x.DosageHours)
                .WithConversion(new DosageHoursConverter());

            // jeśli bind ma być do innej property to wskazuje się tak jak poniżej (metoda .For)
            bset.Bind(time)
                .To(x => x.DosageHours)
                .For(v => v.Visibility)
                .WithConversion(new InlineValueConverter<IEnumerable<TimeSpan>, ViewStates>(dosageHours => dosageHours.Any() ? ViewStates.Visible : ViewStates.Gone));

            bset.Bind(daysOfWeek)
				.To(x => x.Days)
                .WithConversion(new DaysOfWeekConverter());

			bset.Bind(daysOfWeek)
				.To(x => x.Days)
                .For(v => v.Visibility)
                .WithConversion(new InlineValueConverter<DaysOfWeek, ViewStates>(dosageHours => dosageHours == DaysOfWeek.None ? ViewStates.Gone : ViewStates.Visible));
            
			bset.Apply();


            var medication = dataContext as MedicationDosage;
            if (medication?.ThumbnailName != null)
            {
				var thumbnail = view.FindViewById<ImageView>(Resource.Id.list_thumbnail);
				byte[] array = imageLoader.LoadImage(medication.ThumbnailName);
				thumbnail.SetImageBitmap(BitmapFactory.DecodeByteArray(array, 0 ,array.Length));    
            } else {
                var thumbnail = view.FindViewById<ImageView>(Resource.Id.list_thumbnail);
				thumbnail.SetImageBitmap(BitmapFactory.DecodeResource(this.Context.Resources, Resource.Drawable.pill64x64));
			}

			return view;
		}
	}
}
