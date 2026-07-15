using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyAI.Contracts.Common;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LearningController : ControllerBase
{
    private readonly AppDbContext _context;

    public LearningController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("quiz")]
    public async Task<ActionResult<ApiResponse<QuizModel>>> GenerateQuiz([FromBody] LearningRequest request)
    {
        var member = await _context.FamilyMembers.FindAsync(request.MemberId);
        if (member == null)
        {
            return BadRequest(ApiResponse<QuizModel>.FailureResponse("Family member not found."));
        }

        // Return a customized mock educational quiz based on kid's age/class
        var quiz = new QuizModel
        {
            Title = $"Fun Quiz for {member.Nickname}",
            Questions = new List<QuizQuestion>
            {
                new() {
                    Text = "If you have 4 apples and a friend gives you 5 more, how many apples do you have?",
                    Options = new List<string> { "7", "8", "9", "10" },
                    CorrectIndex = 2,
                    Explanation = "Simple addition! 4 + 5 equals 9."
                },
                new() {
                    Text = "Which planet is known as the Red Planet?",
                    Options = new List<string> { "Earth", "Mars", "Jupiter", "Venus" },
                    CorrectIndex = 1,
                    Explanation = "Mars is called the Red Planet because of iron minerals in its soil."
                },
                new() {
                    Text = "How many legs does a spider have?",
                    Options = new List<string> { "6", "8", "10", "12" },
                    CorrectIndex = 1,
                    Explanation = "All spiders are arachnids and have exactly 8 legs."
                }
            }
        };

        return Ok(ApiResponse<QuizModel>.SuccessResponse(quiz, "Quiz generated successfully."));
    }

    [HttpPost("story")]
    public async Task<ActionResult<ApiResponse<StoryModel>>> GenerateStory([FromBody] LearningRequest request)
    {
        var member = await _context.FamilyMembers.FindAsync(request.MemberId);
        if (member == null)
        {
            return BadRequest(ApiResponse<StoryModel>.FailureResponse("Family member not found."));
        }

        var story = new StoryModel
        {
            Title = "The Curious Little Robot",
            Content = $"Once upon a time, in a bright valley, there lived a friendly little learning robot named Dost. Dost loved helping children. One day, he met a child named {member.Nickname} who was {member.Age} years old. {member.Nickname} wanted to learn all about the stars. Dost took {member.Nickname} on a magic rocket ship ride. They counted 10 bright stars together. {member.Nickname} realized that math is not just numbers, it is a key to exploring the universe! The end."
        };

        return Ok(ApiResponse<StoryModel>.SuccessResponse(story, "Story generated successfully."));
    }
}

public record LearningRequest(Guid MemberId);

public class QuizModel
{
    public string Title { get; set; } = string.Empty;
    public List<QuizQuestion> Questions { get; set; } = new();
}

public class QuizQuestion
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class StoryModel
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
