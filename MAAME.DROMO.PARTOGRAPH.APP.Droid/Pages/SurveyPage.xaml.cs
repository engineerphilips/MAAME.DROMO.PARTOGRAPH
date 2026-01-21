using MAAME.DROMO.PARTOGRAPH.MODEL;
using System.Net.Http.Json;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

/// <summary>
/// Survey page for collecting user satisfaction feedback
/// Supports POC Objective 2: User Satisfaction (4/5 target)
/// </summary>
public partial class SurveyPage : ContentPage
{
    private int _easeOfUseRating = 0;
    private int _workflowRating = 0;
    private int _alertsRating = 0;
    private int _trainingRating = 0;
    private int _recommendRating = 0;
    private bool? _patientCareImproved;
    private DateTime _startTime;

    private const string STAR_FILLED = "\u2605";  // Filled star
    private const string STAR_EMPTY = "\u2606";   // Empty star

    public SurveyPage()
    {
        InitializeComponent();
        _startTime = DateTime.UtcNow;
        InitializeStars();
    }

    private void InitializeStars()
    {
        UpdateStarDisplay("EaseOfUse", _easeOfUseRating);
        UpdateStarDisplay("Workflow", _workflowRating);
        UpdateStarDisplay("Alerts", _alertsRating);
        UpdateStarDisplay("Training", _trainingRating);
        UpdateStarDisplay("Recommend", _recommendRating);
    }

    private void UpdateStarDisplay(string category, int rating)
    {
        Color filledColor = Color.FromArgb("#F59E0B"); // Amber/Gold
        Color emptyColor = Color.FromArgb("#CBD5E1");  // Gray

        for (int i = 1; i <= 5; i++)
        {
            var button = this.FindByName<Button>($"{category}{i}");
            if (button != null)
            {
                button.Text = i <= rating ? STAR_FILLED : STAR_EMPTY;
                button.TextColor = i <= rating ? filledColor : emptyColor;
            }
        }

        // Update label
        var label = this.FindByName<Label>($"{category}Label");
        if (label != null)
        {
            label.Text = rating == 0 ? "Not rated" : GetRatingText(rating);
            label.TextColor = rating >= 4 ? Color.FromArgb("#16A34A") :
                              rating >= 3 ? Color.FromArgb("#F59E0B") :
                              rating > 0 ? Color.FromArgb("#DC2626") :
                              Color.FromArgb("#64748B");
        }
    }

    private string GetRatingText(int rating)
    {
        return rating switch
        {
            5 => "Excellent",
            4 => "Good",
            3 => "Average",
            2 => "Poor",
            1 => "Very Poor",
            _ => "Not rated"
        };
    }

    private void OnEaseOfUseRating(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out int rating))
        {
            _easeOfUseRating = rating;
            UpdateStarDisplay("EaseOfUse", rating);
        }
    }

    private void OnWorkflowRating(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out int rating))
        {
            _workflowRating = rating;
            UpdateStarDisplay("Workflow", rating);
        }
    }

    private void OnAlertsRating(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out int rating))
        {
            _alertsRating = rating;
            UpdateStarDisplay("Alerts", rating);
        }
    }

    private void OnTrainingRating(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out int rating))
        {
            _trainingRating = rating;
            UpdateStarDisplay("Training", rating);
        }
    }

    private void OnRecommendRating(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out int rating))
        {
            _recommendRating = rating;
            UpdateStarDisplay("Recommend", rating);
        }
    }

    private void OnPatientCareYes(object sender, EventArgs e)
    {
        _patientCareImproved = true;
        PatientCareYes.BackgroundColor = Color.FromArgb("#16A34A");
        PatientCareYes.TextColor = Colors.White;
        PatientCareNo.BackgroundColor = Color.FromArgb("#FEE2E2");
        PatientCareNo.TextColor = Color.FromArgb("#991B1B");
    }

    private void OnPatientCareNo(object sender, EventArgs e)
    {
        _patientCareImproved = false;
        PatientCareNo.BackgroundColor = Color.FromArgb("#DC2626");
        PatientCareNo.TextColor = Colors.White;
        PatientCareYes.BackgroundColor = Color.FromArgb("#DCFCE7");
        PatientCareYes.TextColor = Color.FromArgb("#166534");
    }

    private async void OnSubmitSurvey(object sender, EventArgs e)
    {
        // Validate that at least the main ratings are provided
        if (_easeOfUseRating == 0 || _workflowRating == 0 || _alertsRating == 0 ||
            _trainingRating == 0 || _recommendRating == 0)
        {
            await DisplayAlert("Incomplete Survey",
                "Please rate all questions before submitting.", "OK");
            return;
        }

        SubmitButton.IsEnabled = false;
        SubmitButton.Text = "Submitting...";

        try
        {
            var completionTime = (int)(DateTime.UtcNow - _startTime).TotalSeconds;

            // Calculate overall satisfaction score
            decimal overallScore = (_easeOfUseRating + _workflowRating + _alertsRating +
                                   _trainingRating + _recommendRating) / 5.0m;

            // Get current staff information from preferences
            var staffId = Preferences.Get("StaffID", string.Empty);
            var staffName = Preferences.Get("StaffName", "Unknown");
            var staffRole = Preferences.Get("StaffRole", "Unknown");
            var facilityId = Preferences.Get("FacilityID", string.Empty);
            var facilityName = Preferences.Get("FacilityName", "Unknown");
            var region = Preferences.Get("Region", "Unknown");
            var district = Preferences.Get("District", "Unknown");

            var response = new SurveyResponse
            {
                ID = Guid.NewGuid(),
                StaffID = Guid.TryParse(staffId, out var sid) ? sid : null,
                FacilityID = Guid.TryParse(facilityId, out var fid) ? fid : null,
                StaffName = staffName,
                StaffRole = staffRole,
                FacilityName = facilityName,
                Region = region,
                District = district,
                OverallSatisfactionScore = Math.Round(overallScore, 2),
                EaseOfUseScore = _easeOfUseRating,
                WorkflowImpactScore = _workflowRating,
                PerceivedBenefitsScore = _alertsRating,
                TrainingAdequacyScore = _trainingRating,
                RecommendationScore = _recommendRating,
                AdditionalComments = CommentsEditor.Text ?? string.Empty,
                SubmittedAt = DateTime.UtcNow,
                CompletionTimeSeconds = completionTime,
                DeviceId = DeviceInfo.Current.Idiom.ToString(),
                AppVersion = AppInfo.Current.VersionString,
                Answers = new List<SurveyAnswer>
                {
                    new SurveyAnswer
                    {
                        QuestionOrder = 1,
                        QuestionText = "How easy is it to use MAAME-DROMO for labor monitoring?",
                        Category = "EaseOfUse",
                        RatingValue = _easeOfUseRating
                    },
                    new SurveyAnswer
                    {
                        QuestionOrder = 2,
                        QuestionText = "How well does MAAME-DROMO integrate into your daily workflow?",
                        Category = "WorkflowImpact",
                        RatingValue = _workflowRating
                    },
                    new SurveyAnswer
                    {
                        QuestionOrder = 3,
                        QuestionText = "How helpful are the automated alerts and recommendations?",
                        Category = "PerceivedBenefits",
                        RatingValue = _alertsRating
                    },
                    new SurveyAnswer
                    {
                        QuestionOrder = 4,
                        QuestionText = "How adequate was the training you received?",
                        Category = "TrainingAdequacy",
                        RatingValue = _trainingRating
                    },
                    new SurveyAnswer
                    {
                        QuestionOrder = 5,
                        QuestionText = "How likely are you to recommend MAAME-DROMO to colleagues?",
                        Category = "Recommendation",
                        RatingValue = _recommendRating
                    },
                    new SurveyAnswer
                    {
                        QuestionOrder = 6,
                        QuestionText = "Has MAAME-DROMO improved patient care at your facility?",
                        Category = "PerceivedBenefits",
                        BoolValue = _patientCareImproved
                    }
                },
                CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Try to submit to API
            bool submitted = await SubmitSurveyToApi(response);

            if (submitted)
            {
                // Store last survey date
                Preferences.Set("LastSurveyDate", DateTime.UtcNow.ToString("O"));

                await DisplayAlert("Thank You!",
                    $"Your feedback has been submitted successfully.\nOverall satisfaction: {overallScore:F1}/5.0",
                    "OK");

                await Navigation.PopAsync();
            }
            else
            {
                // Store locally for later sync
                await StoreSurveyLocally(response);

                await DisplayAlert("Saved Locally",
                    "Your feedback has been saved and will be submitted when you're online.",
                    "OK");

                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                $"An error occurred while submitting the survey. Please try again.\n{ex.Message}",
                "OK");
        }
        finally
        {
            SubmitButton.IsEnabled = true;
            SubmitButton.Text = "Submit Survey";
        }
    }

    private async Task<bool> SubmitSurveyToApi(SurveyResponse response)
    {
        try
        {
            var apiUrl = Preferences.Get("ApiBaseUrl", "http://192.168.100.4:5218");
            using var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // First, get active survey ID (or use default)
            var surveyId = await GetActiveSurveyId(httpClient);

            var httpResponse = await httpClient.PostAsJsonAsync($"api/poc/surveys/{surveyId}/responses", response);
            return httpResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error submitting survey: {ex.Message}");
            return false;
        }
    }

    private async Task<Guid> GetActiveSurveyId(HttpClient httpClient)
    {
        try
        {
            var surveys = await httpClient.GetFromJsonAsync<List<UserSurvey>>("api/poc/surveys");
            var activeSurvey = surveys?.FirstOrDefault(s => s.IsActive);
            if (activeSurvey != null)
                return activeSurvey.ID;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting active survey: {ex.Message}");
        }

        // Return a default GUID if no survey found (API will handle creating one)
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    private async Task StoreSurveyLocally(SurveyResponse response)
    {
        try
        {
            // Store in local database or preferences for later sync
            var pendingSurveys = Preferences.Get("PendingSurveys", "[]");
            var surveys = System.Text.Json.JsonSerializer.Deserialize<List<SurveyResponse>>(pendingSurveys)
                ?? new List<SurveyResponse>();
            surveys.Add(response);
            Preferences.Set("PendingSurveys", System.Text.Json.JsonSerializer.Serialize(surveys));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error storing survey locally: {ex.Message}");
        }
    }

    private async void OnSkipSurvey(object sender, TappedEventArgs e)
    {
        var result = await DisplayAlert("Skip Survey",
            "Are you sure you want to skip the survey? Your feedback helps us improve MAAME-DROMO.",
            "Skip", "Continue Survey");

        if (result)
        {
            // Store skip date to show survey again later
            Preferences.Set("LastSurveySkipDate", DateTime.UtcNow.ToString("O"));
            await Navigation.PopAsync();
        }
    }
}
