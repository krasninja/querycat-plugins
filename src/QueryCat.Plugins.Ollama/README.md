# Ollama Plugin for QueryCat

Provides an [Ollama](https://ollama.com) answer agent that lets QueryCat answer
natural language questions using a locally running LLM.

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

## Setup

1. Install and start [Ollama](https://ollama.com/download).
2. Pull a model, for example: `ollama pull llama3.2`.

## Usage

Create the agent with `ollama_agent(model, url)` and pass it to `ai_input`:

```sql
SELECT * FROM ai_input('How many users are older than 30?', '/path/to/users.csv', ollama_agent('llama3.2'));
```

By default the plugin connects to `http://localhost:11434`. Pass the second
argument to use another server:

```sql
SELECT * FROM ai_input('Show top products', '/path/to/sales.csv', ollama_agent('llama3.2', 'http://192.168.1.10:11434'));
```
