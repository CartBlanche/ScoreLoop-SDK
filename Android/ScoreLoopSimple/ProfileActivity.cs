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
    public class ProfileActivity : Android.App.Activity
    {
        EditText usernameText;
        EditText emailText;

        // identifiers for our dialogues
	    const int	DIALOG_PROGRESS		= 0;
	    const int	DIALOG_SUCCESS  	= 1;
	    const int	DIALOG_ERROR		= 2;

        // saves the last error occurred, so we can read it in onPrepareDialog()
        String dialogErrorMsg = "";
        String dialogSuccessMsg = "";

        class ProfileRequestControllerObserver : Java.Lang.Object, IRequestControllerObserver
        {
            ProfileActivity _Activity;

            public ProfileRequestControllerObserver(ProfileActivity activity)
            {
                _Activity = activity;
            }

            public void RequestControllerDidFail(RequestController controller, Java.Lang.Exception exception)
            {
                _Activity.DismissDialog(DIALOG_PROGRESS);
						
                // Error handling has to account for many different types of
                // failures...		
                if(exception is RequestControllerException) {
						
	                RequestControllerException ctrlException = (RequestControllerException) exception;
							
	                if(ctrlException.HasDetail(RequestControllerException.DetailUserUpdateRequestEmailTaken)) {
		                // this case is not quite a fatal error. if the email address is already
		                // taken, an email will be sent to it to allow the user to link this device
		                // with his account. 
		                // that's why we'll show a success dialog in this case.
		                _Activity.dialogSuccessMsg = _Activity.GetString(Resource.String.profile_success_email_taken);
		                _Activity.ShowDialog(DIALOG_SUCCESS);
	                }
	                else {
		                // in any of these cases it's an error:
								
		                _Activity.dialogErrorMsg = "";
		                // email may be invalid
		                if(ctrlException.HasDetail(RequestControllerException.DetailUserUpdateRequestInvalidEmail)) {
			                _Activity.dialogErrorMsg += _Activity.GetString(Resource.String.profile_error_email_invalid);
		                }
								
		                // username may be invalid, taken or too short
		                if(ctrlException.HasDetail(RequestControllerException.DetailUserUpdateRequestUsernameTaken)) {
			                _Activity.dialogErrorMsg += _Activity.GetString(Resource.String.profile_error_username_taken);
		                }
                        else if (ctrlException.HasDetail(RequestControllerException.DetailUserUpdateRequestUsernameTooShort))
                        {
			                _Activity.dialogErrorMsg += _Activity.GetString(Resource.String.profile_error_username_too_short);
		                }
                        else if (ctrlException.HasDetail(RequestControllerException.DetailUserUpdateRequestInvalidUsername))
                        {
                            _Activity.dialogErrorMsg += _Activity.GetString(Resource.String.profile_error_username_invalid);
		                }

                        _Activity.ShowDialog(DIALOG_ERROR);
	                }
                }
                else {
	                // generic Exception
                    _Activity.dialogErrorMsg = exception.LocalizedMessage;
                    _Activity.ShowDialog(DIALOG_ERROR);
                }
						

                // update displayed values
                User user = ((UserController)controller).User;
                _Activity.usernameText.Text = user.Login;
                _Activity.emailText.Text = user.EmailAddress;
            }

            public void RequestControllerDidReceiveResponse(RequestController p0)
            {
                _Activity.DismissDialog(DIALOG_PROGRESS);

                _Activity.dialogSuccessMsg = _Activity.GetString(Resource.String.profile_success);
						
                _Activity.ShowDialog(DIALOG_SUCCESS);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "gameplay" layout resource
            SetContentView(Resource.Layout.profile);

            // find our text fields
            usernameText = FindViewById<EditText>(Resource.Id.text_username);
            emailText = FindViewById<EditText>(Resource.Id.text_email);

            Button button_save_profile = FindViewById<Button>(Resource.Id.button_save_profile);
            button_save_profile.Click += delegate {
                // get the current User
                User user = Session.CurrentSession.User;

                // update his values
                user.Login = usernameText.Text;
                user.EmailAddress = emailText.Text;

                // set up a request observer
                ProfileRequestControllerObserver observer = new ProfileRequestControllerObserver(this);

                // with our observer, set up the request controller
				UserController userController = new  UserController(observer);
				
				// pass the user into the controller
				userController.SetUser(user);

                ShowDialog(DIALOG_PROGRESS);

                // submit our changes
                userController.SubmitUser();
            };
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
                    e.SetMessage("");
			        return e.Create();            
		        case DIALOG_SUCCESS:
                    var s = new AlertDialog.Builder(this);			      
				        s.SetTitle(Resource.String.scoreloop);
				        s.SetIcon(Resources.GetDrawable(Resource.Drawable.Icon));
                        s.SetPositiveButton(Resource.String.awesome, delegate { });
                        s.SetMessage("");
				    return s.Create();
            }

            return null;
        }

        protected override void  OnPrepareDialog(int id, Dialog dialog)
        {
            switch (id)
            {
                case DIALOG_ERROR:
                    AlertDialog errorDialog = (AlertDialog)dialog;
			        errorDialog.SetMessage(dialogErrorMsg);
                    break;
                case DIALOG_SUCCESS:
                    AlertDialog successDialog = (AlertDialog)dialog;
			        successDialog.SetMessage(dialogSuccessMsg);
                    break;
            }
        }
    }
}

