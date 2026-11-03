using System.Text.Json;

namespace AgentFlow.Application.Planning;

public static class PlanningJsonSchema
{
    private const string Schema = """
    {
      "type": "object",
      "properties": {
        "tasks": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "key": {
                "type": "string"
              },
              "title": {
                "type": "string"
              },
              "description": {
                "type": "string"
              },
              "parentKey": {
                "type": ["string", "null"]
              }
            },
            "required": [
              "key",
              "title",
              "description",
              "parentKey"
            ],
            "additionalProperties": false
          }
        },
        "dependencies": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "taskKey": {
                "type": "string"
              },
              "dependsOnTaskKey": {
                "type": "string"
              }
            },
            "required": [
              "taskKey",
              "dependsOnTaskKey"
            ],
            "additionalProperties": false
          }
        }
      },
      "required": [
        "tasks",
        "dependencies"
      ],
      "additionalProperties": false
    }
    """;

    public static JsonElement GetSchema()
    {
        using var document = JsonDocument.Parse(Schema);

        return document.RootElement.Clone();
    }
}