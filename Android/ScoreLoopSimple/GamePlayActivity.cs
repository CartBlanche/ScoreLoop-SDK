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

namespace AndroidApplication12
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon")]
    public class GamePlayActivity : Android.App.Activity
    {
        EditText scoreField;

        // identifiers for our dialogues
	    const int	DIALOG_PROGRESS		= 0;
	    const int	DIALOG_SUBMITTED	= 1;
	    const int	DIALOG_FAILED		= 2;

        class SubmitScoreRequestControllerObserver : Java.Lang.Object, IRequestControllerObserver
        {
            GamePlayActivity _Activity;

            public SubmitScoreRequestControllerObserver(GamePlayActivity activity)
            {
                _Activity = activity;
            }

            public void RequestControllerDidFail(RequestController p0, Java.Lang.Exception p1)
            {
                // something went wrong... possibly no internet connection
                _Activity.DismissDialog(DIALOG_PROGRESS);
                _Activity.ShowDialog(DIALOG_FAILED);
            }

            public void RequestControllerDidReceiveResponse(RequestController p0)
            {
                // reset the text field to 0
                _Activity.scoreField.Text = "0";
                // remove the progress dialog
                _Activity.DismissDialog(DIALOG_PROGRESS);
                // show the success dialog
                _Activity.ShowDialog(DIALOG_SUBMITTED);
                // alternatively, you may want to return to the main screen
                // or start another round of the game at this point
            }

           
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "gameplay" layout resource
            SetContentView(Resource.Layout.gameplay);

            // Get our button from the layout resource,
            // and attach an event to it
            scoreField = FindViewById<EditText>(Resource.Id.scoreText);

            Button button_score1 = FindViewById<Button>(Resource.Id.button_score1);
            button_score1.Click += delegate { addScore(1); };

            Button button_score10 = FindViewById<Button>(Resource.Id.button_score10);
            button_score10.Click += delegate { addScore(10); };

            Button button_score100 = FindViewById<Button>(Resource.Id.button_score100);
            button_score100.Click += delegate { addScore(100); };

            Button button_game_over = FindViewById<Button>(Resource.Id.button_game_over);
            button_game_over.Click += delegate { 
                // read score from text field
				double scoreResult;
				try {
                    scoreResult = double.Parse(scoreField.Text);
				} catch(Exception) {
					scoreResult = 0;
				}

				// this is where you should input your game's score
                Score score = new Score((Java.Lang.Double)scoreResult, null);

				// set up an observer for our request
				SubmitScoreRequestControllerObserver observer = new SubmitScoreRequestControllerObserver(this);

				// with the observer, we can create a ScoreController to submit the score
				ScoreController scoreController = new ScoreController(observer);
            
				// show a progress dialog while we are submitting
				ShowDialog(DIALOG_PROGRESS);

				// this is the call that submits the score
				scoreController.SubmitScore(score);
				// please note that the above method will return immediately and reports to
				// the RequestControllerObserver when it's done/failed
            };

           
        }

        protected override Dialog OnCreateDialog(int id)
        {
            switch (id)
            {
                case DIALOG_PROGRESS:
                    return ProgressDialog.Show(this, "", GetString(Resource.String.submitting_your_score));
                case DIALOG_FAILED:
                    var e = new AlertDialog.Builder(this);
                    e.SetPositiveButton(Resource.String.too_bad, delegate { });
                    e.SetMessage(Resource.String.score_submit_error);
                    return e.Create();
                case DIALOG_SUBMITTED:
                    var s = new AlertDialog.Builder(this);
                    s.SetTitle(Resource.String.scoreloop);
                    s.SetIcon(Resources.GetDrawable(Resource.Drawable.Icon));
                    s.SetPositiveButton(Resource.String.awesome, delegate { });
                    s.SetMessage(Resource.String.score_was_submitted);
                    return s.Create();
            }

            return null;
        }

        // adds some points to the score in the text field
        public void addScore(int points)
        {
            int old = int.Parse(scoreField.Text);
            scoreField.Text = "" + (old + points);
        }
    }
}

