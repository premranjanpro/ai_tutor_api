using FamilyAI.Domain.Entities;
using FamilyAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FamilyAI.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Apply migrations if not InMemory
        if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            await context.Database.MigrateAsync();
        }

        // Seed default prompt templates
        if (!await context.AiPromptTemplates.AnyAsync(p => p.PromptCode == "child_conversation"))
        {
            var childPrompt = new AiPromptTemplate
            {
                PromptCode = "child_conversation",
                PromptName = "Child General Conversation",
                Provider = "Gemini",
                Model = "gemini-1.5-flash",
                SystemPrompt = @"You are a friendly AI learning companion.

You are speaking with:
Name: {{member_name}}
Age: {{age}}
Preferred language: {{preferred_language}}
Class: {{class_name}}
Interests: {{interests}}
Learning goals: {{learning_goals}}

Current date and time:
{{current_datetime}}

Today's study plan:
{{today_study_plan}}

Approved memories:
{{approved_memories}}

Conversation rules:
1. Clearly behave as an AI assistant and never claim to be human.
2. Speak using simple and age-appropriate language.
3. Reply in the same language the user speaks. If the user speaks in Hindi, reply in Hindi. If the user speaks in English, reply in English.
4. CRITICAL: If the child's age is greater than 5 years, gradually transition the conversation to English to help them learn English. Use simple English words and explain them, even if the child speaks in Hindi.
5. Ask only one clear question at a time.
6. Keep normal responses under five short sentences.
7. Encourage effort instead of only praising correct answers.
8. Never insult, shame, threaten or frighten the child.
9. Never ask the child to keep secrets from parents or guardians.
10. Do not reveal another family member's conversations or memories.
11. If the child describes danger, abuse, serious illness or self-harm, encourage contacting a trusted adult immediately and generate a safety escalation event.
12. Do not diagnose health conditions.
13. Ask before changing the study plan.
14. Do not save sensitive information unless parent consent exists.
15. Use approved memories naturally, but do not mention everything you remember.
16. When teaching, first understand the child's current knowledge.
17. Provide hints before providing the final answer.
18. End study sessions with a short recap.",
                Temperature = 0.7,
                MaxTokens = 800,
                IsActive = true
            };

            await context.AiPromptTemplates.AddAsync(childPrompt);
        }

        // Seed default test user in database
        var testEmail = "test@family.com";
        if (!await context.Users.AnyAsync(u => u.Email == testEmail))
        {
            var testUser = new User
            {
                FullName = "Test Parent",
                Email = testEmail,
                MobileNumber = "9999999999",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "FamilyAdmin"
            };

            var testFamily = new Family
            {
                FamilyName = "Test Family",
                PrimaryLanguage = "Hindi",
                Country = "India"
            };

            var parentMember = new FamilyMember
            {
                FamilyId = testFamily.Id,
                UserId = testUser.Id,
                FullName = "Test Parent",
                Nickname = "Papa",
                Relation = "Father",
                MemberType = MemberType.Parent,
                PreferredLanguage = "Hindi"
            };

            var childMember = new FamilyMember
            {
                FamilyId = testFamily.Id,
                FullName = "Aarav Ranjan",
                Nickname = "Aarav",
                Relation = "Son",
                MemberType = MemberType.Child,
                DateOfBirth = DateTime.UtcNow.AddYears(-8), // 8 years old (> 5 trigger active)
                PreferredLanguage = "Hindi",
                ClassName = "Class 3",
                SchoolBoard = "CBSE",
                InterestsJson = JsonSerializer.Serialize(new List<string> { "Dinosaurs", "Space" }),
                LearningGoalsJson = JsonSerializer.Serialize(new List<string> { "Learn multiplication table of 7" })
            };

            testFamily.Members.Add(parentMember);
            testFamily.Members.Add(childMember);

            await context.Users.AddAsync(testUser);
            await context.Families.AddAsync(testFamily);
        }

        await context.SaveChangesAsync();
    }
}
