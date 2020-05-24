using Blazorlingo.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace Blazorlingo.Client.Shared
{
    public class SessionViewerComponent : ComponentBase
    {
        [Inject]
        HttpClient Http { get; set; }

        protected Session currentSession = null;
        protected SimpleChallenge currentChallenge = new SimpleChallenge();
        protected Course currentCourse = null;

        protected bool loading = true;
        protected int challengeIndex = 0;

        protected override async Task OnInitializedAsync()
        {
        }

        public async Task LoadCourse(Course course)
        {
            loading = true;
            currentChallenge.Question = $"Loading {course.Title}...";
            currentChallenge.Answer = "";
            StateHasChanged();
            await Http.PostAsync($"duolingo/loadCourse/{course.Id}/{course.FromLanguage}/{course.LearningLanguage}", null);
            currentChallenge.Question = $"Loading...";
            StateHasChanged();
            await LoadSession(course);
        }

        private async Task LoadSession(Course course)
        {
            loading = true;
            currentCourse = course;
            currentChallenge.Answer = "";
            StateHasChanged();
            var session = await Http.GetFromJsonAsync<Session>($"duolingo/session/{course.FromLanguage}/{course.LearningLanguage}");
            currentSession = session;
            challengeIndex = 0;
            currentChallenge = currentSession.Challenges[challengeIndex];
            loading = false;
            StateHasChanged();
        }

        protected async Task Continue()
        {
            challengeIndex++;

            if (currentSession.Challenges.Length == challengeIndex)
            {
                loading = true;
                currentChallenge.Question = $"Gaining XP...";
                currentChallenge.Answer = "";
                StateHasChanged();
                var result = await Http.GetFromJsonAsync<SessionOutcome>("duolingo/complete");
                currentChallenge.Question = $"+{result.XpGain} XP, loading...";
                await LoadSession(currentCourse);
            }
            else
                currentChallenge = currentSession.Challenges[challengeIndex];
        }
    }
}
