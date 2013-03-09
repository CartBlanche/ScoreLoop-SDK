using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Com.Scoreloop.Client;
using Android.Util;

namespace AndroidApplication12
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        String secret = "2PCCEsjTS+Wty7t6ZC0kJrOuB7dNbloP7zeSBErM8dYqJgowqWzn9A==";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Log.Debug("Tag", "This is a test");

            // initialize the client using the context and game secret
            Com.Scoreloop.Client.Android.Core.Model.Client.Init(this, secret, null);

            var uri = ContactsContract.Contacts.ContentUri;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button_start_gane = FindViewById<Button>(Resource.Id.button_start_gane);
            button_start_gane.Click += delegate { StartActivity(typeof(GamePlayActivity)); };

            Button button_leaderboard = FindViewById<Button>(Resource.Id.button_leaderboard);
            button_leaderboard.Click += delegate { StartActivity(typeof(LeaderboardActivity)); };

            Button button_profile = FindViewById<Button>(Resource.Id.button_profile);
            button_profile.Click += delegate { StartActivity(typeof(ProfileActivity)); };
        }
    }
}

