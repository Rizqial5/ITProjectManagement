using Microsoft.AspNetCore.Razor.TagHelpers;
using ProjectManagement.App.Models.Enum;

namespace ProjectManagement.App.TagHelper
{
    [HtmlTargetElement("status-badge")]
    public class StatusBadgeHelper : ITagHelper
    {
        public int Order => 0;

        public Status status { get; set; }

        public void Init(TagHelperContext context)
        {

        }

        public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var cssClass = status switch
            {
                Status.ToDo => "badge bg-secondary",
                Status.InProgress => "badge bg-warning text-dark",
                Status.Done => "badge bg-success",
                _ => "badge bg-light text-dark"
            };

            output.TagName = "span";
            output.Attributes.SetAttribute("class", cssClass);
            output.Content.SetContent(status.ToString());

            return Task.CompletedTask;
        } 
    }
}
