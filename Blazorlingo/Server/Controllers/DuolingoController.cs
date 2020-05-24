using Blazorlingo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Blazorlingo.Server.Clients;
using Refit;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Blazorlingo.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DuolingoController : ControllerBase
    {
        private readonly IDuolingoApi _api;
        private readonly IOptions<DuolingoSettings> _settings;

        private static GetSessionResponse currentSession;
        
        public DuolingoController(IDuolingoApi api, IOptions<DuolingoSettings> settings)
        {
            _settings = settings;
            _api = api;
        }

        [HttpPost]
        [Route("loadCourse/{courseId}/{fromLanguage}/{learningLanguage}")]
        public async Task<IActionResult> LoadCourse(string courseId, string fromLanguage, string learningLanguage)
        {
            await _api.LoadCourse(_settings.Value.UserId, new LoadCourseRequest()
            {
                CourseId = courseId,
                FromLanguage = fromLanguage,
                LearningLanguage = learningLanguage
            });
            return Ok();
        }

        [HttpGet]
        [Route("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _api.GetCourses(_settings.Value.UserId);
            return Ok(courses.Courses.OrderByDescending(x => x.Xp));
        }

        [HttpGet]
        [Route("complete")]
        public async Task<IActionResult> Complete()
        {
            var sessionId = currentSession.Id;

            var resp = await _api.CompleteSession(sessionId, new CompleteSessionRequest()
            {   
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                DisableBonusPoints = true,
                FromLanguage = currentSession.FromLanguage,
                LearningLanguage = currentSession.LearningLanguage,
                Failed = false,
                HeartsLeft = 0,
                MaxInLessonStreak = (currentSession.Challenges as JArray).Count,
                Type = "GLOBAL_PRACTICE",
                Id = sessionId,
                StartTime = currentSession.StartTime,
                Challenges = currentSession.Challenges,
                Metadata = currentSession.Metadata,
                TrackingProperties = currentSession.TrackingProperties
            });

            return Ok(new SessionOutcome() { XpGain = resp.XpGain });
        }

        [HttpGet]
        [Route("currentcourse")]
        public async Task<IActionResult> GetCurrentCourse()
        {
            var currentCourse = await _api.GetCurrentCourse(_settings.Value.UserId);
            return Ok(currentCourse.CurrentCourse);
        }

        [HttpGet]
        [Route("session/{fromLanguage}/{learningLanguage}")]
        public async Task<IActionResult> GetSession(string fromLanguage, string learningLanguage)
        {
            var session = await _api.GetSession(new GetSessionRequest()
            {
                ChallengeTypes = new string[] { "characterIntro", "characterMatch", "characterSelect", "dialogue", "form", "judge", "listen", "name", "listenComprehension", "listenTap", "readComprehension", "select", "selectPronunciation", "selectRecording", "selectTranscription", "translate" },
                FromLanguage = fromLanguage,
                LearningLanguage = learningLanguage,
                Type = "GLOBAL_PRACTICE"
            });

            currentSession = session;
            currentSession.StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var sessionChallenges = (session.Challenges as JArray).ToObject<List<Challenge>>();

            var challenges = new List<SimpleChallenge>();

            foreach (var challenge in sessionChallenges)
            {
                switch (challenge.Type)
                {
                    case "form":
                        challenges.Add(new SimpleChallenge() { Question = challenge.Metadata.correct_solutions[0], Answer = challenge.SolutionTranslation });
                        break;
                    case "translate":
                        challenges.Add(new SimpleChallenge() { Question = challenge.Prompt, Answer = challenge.CorrectSolutions[0] });
                        break;
                    case "judge":
                        challenges.Add(new SimpleChallenge() { Question = challenge.Prompt, Answer = challenge.Choices[challenge.CorrectIndices[0]].ToString() });
                        break;
                    case "listenTap":
                    case "listen":
                        challenges.Add(new SimpleChallenge() { Question = challenge.Prompt, Answer = challenge.SolutionTranslation });
                        break;
                    case "name":
                        challenges.Add(new SimpleChallenge() { Question = challenge.Prompt, Answer = challenge.CorrectSolutions[0] });
                        break;
                    case "characterIntro":
                        challenges.Add(new SimpleChallenge() { Question = challenge.Prompt, Answer = challenge.Choices[challenge.Metadata["correct_option_index"]].ToString() });
                        break;
                }
            }

            return Ok(new Session() { 
                Id = session.Id, 
                Challenges = challenges.ToArray()
            });
        }
    }
}
