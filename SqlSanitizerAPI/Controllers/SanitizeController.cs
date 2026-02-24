using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSanitizerAPI.Models.Requests;
using SqlSanitizerAPI.Services.SanitizationService;
using System.ComponentModel.DataAnnotations;

namespace SqlSanitizerAPI.Controllers
{
    /// <summary> Provides API endpoints for sanitizing SQL queries and managing sensitive words. </summary>
    /// <param name="sanitizationService">The service responsible for sanitizing SQL queries and managing sensitive words.</param>
    [ApiVersion(1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class SanitizeController(ISanitizationService sanitizationService) : ControllerBase
    {
        private readonly ISanitizationService _sanitizationService = sanitizationService;

        /// <summary> Sanitizes a SQL query by removing or masking sensitive words. </summary>
        /// <param name="sqlQuery">The SQL query to sanitize</param>
        /// <returns>The sanitized SQL query</returns>
        [HttpPost("sanitize")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SanitizeSqlQuery([FromBody][Required][MinLength(1)] string sqlQuery)
        {
            var result = await _sanitizationService.SanitizeSqlQueryAsync(sqlQuery);

            return result.Match<IActionResult>(
                success => Ok(success),
                error => StatusCode(error.ErrorCode, error.ErrorMessage));
        }

        /// <summary> Gets all sensitive words. </summary>
        /// <returns>List of sensitive words</returns>
        [HttpGet("read/sensitive-words")]
        [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSensitiveWords()
        {
            var result = await _sanitizationService.GetSensitiveWordsAsync();

            return result.Match<IActionResult>(
                success => Ok(success),
                error => StatusCode(error.ErrorCode, error.ErrorMessage));
        }

        [HttpGet("details/sensitive-words")]
        [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListSensitiveWords()
        {
            var result = await _sanitizationService.ListSensitiveWordsDetailAsync();

            return result.Match<IActionResult>(
                success => Ok(success),
                error => StatusCode(error.ErrorCode, error.ErrorMessage));
        }

        /// <summary> Adds a new sensitive word to the database. </summary>
        /// <param name="word">The sensitive word to add</param>
        /// <returns>The ID of the newly created word</returns>
        [HttpPost("create/sensitive-words")]
        [ProducesResponseType<int>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateSensitiveWord([FromBody][Required][MinLength(1)] string word)
        {
            var result = await _sanitizationService.InsertSensitiveWordAsync(word);

            return result.Match<IActionResult>(
                success => CreatedAtAction(nameof(GetSensitiveWords), new { id = success }, new { rowsAffected = success }),
                error => StatusCode(error.ErrorCode, error.ErrorMessage));
        }

        /// <summary> Updates an existing sensitive word. </summary>
        /// <param name="request">The update request containing ID and new word value</param>
        /// <returns>Number of rows affected</returns>
        [HttpPut("update/sensitive-words")]
        [ProducesResponseType<int>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSensitiveWord([FromBody] UpdateSanitizeStringRequest request)
        {
            var result = await _sanitizationService.UpdateSensitiveWordAsync(request.Id, request.SanitizeString);

            return result.Match<IActionResult>(
                success => Ok(new { rowsAffected = success }),
                error => StatusCode(error.ErrorCode, error.ErrorMessage));
        }

        /// <summary> Deletes a sensitive word by ID. </summary>
        /// <param name="id">The ID of the word to delete</param>
        /// <returns>Number of rows affected</returns>
        [HttpDelete("delete/sensitive-words/{id:int}")]
        [ProducesResponseType<int>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSensitiveWord([FromRoute][Range(1, int.MaxValue)] int id)
        {
            var result = await _sanitizationService.DeleteSensitiveWordAsync(id);

            return result.Match<IActionResult>(
                success => Ok(new { rowsAffected = success }),
                error => StatusCode(error.ErrorCode, error.ErrorMessage));
        }
    }
}