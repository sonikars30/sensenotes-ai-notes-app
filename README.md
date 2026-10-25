# SenseNotes - AI-Powered Notes App
React + TypeScript, C# .NET 8 API, MongoDB, Python (FastAPI) NLP service.

## Run (4 terminals)
1. MongoDB:   `docker run -p 27017:27017 mongo`
2. ML:        `cd ml-service && pip install -r requirements.txt && uvicorn main:app --port 8000`
3. API:       `cd backend/SenseNotes.Api && dotnet run`   (http://localhost:5000)
4. Frontend:  `cd frontend && npm install && npm run dev` (http://localhost:5173)
Tests: `cd backend && dotnet test SenseNotes.Tests`

## API
POST /api/notes | GET /api/notes?q=pre&page=1 | GET /api/notes/{id} | PUT /api/notes/{id} | DELETE /api/notes/{id}

## Where the data structures live
- LRU cache (hash map + doubly linked list): Services/LruCache.cs
- Trie prefix search: Services/Trie.cs

## No database? No problem
By default the API stores notes in memory, so you do not need MongoDB or Docker to try the app (notes disappear when the API stops).
To use MongoDB: set the setting `Mongo__Conn` to `mongodb://localhost:27017` before `dotnet run`
(Command Prompt: `set Mongo__Conn=mongodb://localhost:27017`).
