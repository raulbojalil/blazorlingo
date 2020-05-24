using Blazorlingo.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blazorlingo.Client.Shared
{
    public class LangNavComponent : ComponentBase
    {
        [Inject]
        HttpClient Http { get; set; }

        [Parameter] public EventCallback<Course> OnLanguageSelected { get; set; }

        protected Course[] courses = new Course[] { };
        protected Course currentCourse = null;

        protected override async Task OnInitializedAsync()
        {
            courses = await Http.GetFromJsonAsync<Course[]>("duolingo/courses");
            currentCourse = await Http.GetFromJsonAsync<Course>("duolingo/currentcourse");
            StateHasChanged();
            await LoadCourse(currentCourse);
        }

        protected async Task LoadCourse(Course course)
        {
            currentCourse = course;
            await OnLanguageSelected.InvokeAsync(course);
        }
    }
}
