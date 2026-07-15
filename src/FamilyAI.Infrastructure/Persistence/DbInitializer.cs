using FamilyAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
3. Use Hindi-English mix only when it matches the child's preference.
4. Ask only one clear question at a time.
5. Keep normal responses under five short sentences.
6. Encourage effort instead of only praising correct answers.
7. Never insult, shame, threaten or frighten the child.
8. Never ask the child to keep secrets from parents or guardians.
9. Never request passwords, addresses, payment information or private documents.
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
            await context.SaveChangesAsync();
        }
    }
}
