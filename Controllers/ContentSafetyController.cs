using Azure;
using Azure.AI.ContentSafety;
using ContentSafetyDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ContentSafetyDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  
    public class ContentSafetyController : ControllerBase
    {
        private readonly IContentSafety _contentSafety;
        private readonly IIntentDetector _intentDetector;

        public ContentSafetyController(IContentSafety contentSafety, IIntentDetector intentDetector)
        {
            _contentSafety = contentSafety;
            _intentDetector = intentDetector;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeText([FromBody] AnalyzeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text is required.");

            var text = request.Text.ToLower();

            // 🔴 STEP 1: INTENT DETECTION (High Priority)

            if (_intentDetector.IsHateIntent(text))
            {
                return Block("Hate intent detected");
            }

            if (_intentDetector.IsSexualIntent(text))
            {
                return Block("Sexual intent detected");
            }

            if (_intentDetector.IsSelfHarmIntent(text))
            {
                return Ok(new
                {
                    isSafe = false,
                    type = "self_harm",
                    message = "User may need support"
                });
            }

            if (_intentDetector.IsSeriousViolence(text))
            {
                return Block("Violence intent detected");
            }

            // 🟡 STEP 2: AZURE CONTENT SAFETY
            var result = await _contentSafety.AnalyzeText(request.Text);

            var categories = result.CategoriesAnalysis
                .Select(x => new
                {
                    Category = x.Category,
                    Severity = x.Severity
                })
                .ToList();

            foreach (var c in categories)
            {
                switch (c.Category)
                {
                    case "hate":
                        if (c.Severity >= 1)
                            return Block("Hate content", categories);
                        break;

                    case "sexual":
                        if (c.Severity >= 1)
                            return Block("Sexual content", categories);
                        break;

                    case "violence":
                        if (c.Severity >= 2)
                            return Block("Violence content", categories);

                        if (_intentDetector.IsMildAggression(text))
                        {
                            return Ok(new
                            {
                                isSafe = false,
                                type = "aggression",
                                message = "User is frustrated",
                                categories
                            });
                        }
                        break;

                    case "self_harm":
                        if (c.Severity >= 1)
                        {
                            return Ok(new
                            {
                                isSafe = false,
                                type = "self_harm",
                                message = "User may need support",
                                categories
                            });
                        }
                        break;
                }
            }

            // 🟢 STEP 3: SAFE CONTENT
            return Ok(new
            {
                isSafe = true,
                categories
            });
        }

        // 🔒 Common block response
        private IActionResult Block(string reason, object categories = null)
        {
            return StatusCode(403, new
            {
                result = "Content blocked by safety policy",
                reason,
                categories
            });
        }


        
    }

    public class AnalyzeRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
