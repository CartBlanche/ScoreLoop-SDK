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

using Com.Scoreloop.Client.Android.Core.Controller;
using Com.Scoreloop.Client.Android.Core.Model;
using System.Collections.Generic;

namespace AndroidApplication12
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon")]
    public class LeaderboardActivity : Android.App.Activity
    {
        // identifiers for our dialogues
	    const int	DIALOG_ERROR		= 0;
	    const int	DIALOG_PROGRESS 	= 1;

        class LeaderboardRequestControllerObserver : Java.Lang.Object, IRequestControllerObserver
        {
            LeaderboardActivity _Activity;

            public LeaderboardRequestControllerObserver(LeaderboardActivity activity)
            {
                _Activity = activity;
            }

            public void RequestControllerDidFail(RequestController controller, Java.Lang.Exception exception)
            {
                _Activity.DismissDialog(DIALOG_PROGRESS);
                _Activity.ShowDialog(DIALOG_ERROR);
            }

            public void RequestControllerDidReceiveResponse(RequestController requestController)
            {
                // get the scores from our controller
				ScoresController scoresController = (ScoresController) requestController;
				var scores = scoresController.Scores;

				// set up the leaderboard as string array
				var scoreList = new List<String>();
				int i = 0;
				foreach ( Score score in scores) {
					scoreList.Add(++i + ". " + score.User.DisplayName + " - " + ScoreFormatter.Format(score));
				}

				// find the list
				ListView list = _Activity.FindViewById<ListView>(Resource.Id.leaderboard_list);

				// set up an adapter for our list view
                var adapter = new ArrayAdapter<String>(_Activity, Android.Resource.Layout.SimpleListItem1, scoreList);

				// put the adapter into the list
				list.Adapter = adapter;

				// we're done!
                _Activity.DismissDialog(DIALOG_PROGRESS);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "gameplay" layout resource
            SetContentView(Resource.Layout.leaderboard);
        }

        protected override Dialog OnCreateDialog(int id)
        {
            switch (id)
            {
                case DIALOG_PROGRESS:
                    return ProgressDialog.Show(this, "", GetString(Resource.String.loading));
                case DIALOG_ERROR:
                    var e = new AlertDialog.Builder(this);
                    e.SetPositiveButton(Resource.String.too_bad, delegate { });
                    e.SetMessage(Resource.String.leaderboard_error);
                    return e.Create();
            }

            return null;
        }

        protected override void OnResume()
        {
            base.OnResume();

            LeaderboardRequestControllerObserver observer = new LeaderboardRequestControllerObserver(this);

            // set up a ScoresController with our observer
            ScoresController scoresController = new ScoresController(observer);

            // show a progress dialog while we're waiting
            ShowDialog(DIALOG_PROGRESS);

            // we want to get the top 10 entries...
            scoresController.RangeLength = 10;

            // starting from the first place
            scoresController.LoadRangeAtRank(1);
        }
    }
}

