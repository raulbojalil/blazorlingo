using Blazorlingo.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazorlingo.Server.Clients
{
    public interface IDuolingoApi
    {
        [Get("/users/{userId}?fields=courses")]
        Task<GetCoursesResponse> GetCourses(string userId);

        [Get("/users/{userId}?fields=currentCourse")]
        Task<GetCurrentCourseResponse> GetCurrentCourse(string userId);

        [Post("/sessions")]
        Task<GetSessionResponse> GetSession([FromBody]GetSessionRequest request);

        [Put("/sessions/{sessionId}")]
        Task<CompleteSessionResponse> CompleteSession(string sessionId, CompleteSessionRequest request);

        [Patch("/users/{userId}?fields=fromLanguage,courses,currentCourse,learningLanguage")]
        Task LoadCourse(string userId, LoadCourseRequest request);
    }

    public class LoadCourseRequest
    {
        public string CourseId { get; set; }
        public string FromLanguage { get; set; }
        public string LearningLanguage { get; set; }

    }

    public class CompleteSessionResponse
    {
        public int XpGain { get; set; }
    }

    public class GetCurrentCourseResponse
    {
        public Course CurrentCourse { get; set; }
    }

    public class CompleteSessionRequest
    {
        public string FromLanguage { get; set; }
        public string LearningLanguage { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public int HeartsLeft { get; set; }
        public long StartTime { get; set; }
        [JsonPropertyName("disable_bonus_points")]
        public bool DisableBonusPoints { get; set; }
        public long EndTime { get; set; }
        public bool Failed { get; set; }
        [JsonPropertyName("max_in_lesson_streak")]
        public long MaxInLessonStreak { get; set; }
        public object Metadata { get; set; }
        public object Challenges { get; set; }
        public object TrackingProperties { get; set; }
    }

    public class GetSessionResponse
    {
        public string Id { get; set; }
        public long StartTime { get; set; }
        public string FromLanguage { get; set; }
        public string LearningLanguage { get; set; }
        public object Metadata { get; set; }
        public object Challenges { get; set; }
        public object TrackingProperties { get; set; }
    }

    public class Challenge
    {
        public object[] Choices { get; set; }
        public int[] CorrectIndices { get; set; }
        public string Prompt { get; set; }
        public string Type { get; set; }
        public string SolutionTranslation { get; set; }
        public string[] CorrectSolutions { get; set; }
        public dynamic Metadata { get; set; }
    }

    public class GetSessionRequest
    {
        public string FromLanguage { get; set; }
        public string LearningLanguage { get; set; }
        public string[] ChallengeTypes { get; set; }
        public string Type { get; set; }
    }

    public class GetCoursesResponse
    {
        public Course[] Courses { get; set; }
    }
}
