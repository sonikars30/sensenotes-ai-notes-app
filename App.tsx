import { useEffect, useState } from "react";
import { createNote, deleteNote, listNotes, MAX_TEXT, MAX_TITLE, Note } from "./api";

type View = "write" | "notes";

export default function App() {
  const [view, setView] = useState<View>("write");
  const [selected, setSelected] = useState<Note | null>(null);

  const open = (n: Note) => { setSelected(n); setView("notes"); };

  return (
    <main>
      <h1>SenseNotes</h1>
      <nav>
        <button className={view === "write" && !selected ? "tab on" : "tab"} onClick={() => { setSelected(null); setView("write"); }}>Write note</button>
        <button className={view === "notes" && !selected ? "tab on" : "tab"} onClick={() => { setSelected(null); setView("notes"); }}>My notes</button>
      </nav>
      {selected ? <Detail note={selected} onBack={() => setSelected(null)} />
        : view === "write" ? <Write onSaved={open} /> : <List onOpen={open} />}
    </main>
  );
}

function Write({ onSaved }: { onSaved: (n: Note) => void }) {
  const [title, setTitle] = useState("");
  const [text, setText] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function save() {
    if (!title.trim() || !text.trim()) return setError("Add a title and some text.");
    if (title.length > MAX_TITLE) return setError(`Title is too long. Max ${MAX_TITLE} characters.`);
    if (text.length > MAX_TEXT) return setError(`Note is too long. Max ${MAX_TEXT} characters.`);
    setError(null); setBusy(true);
    try { onSaved(await createNote(title, text)); }
    catch (e: any) { setError(e.message); }
    finally { setBusy(false); }
  }

  return (
    <section>
      <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Title" />
      <textarea value={text} onChange={(e) => setText(e.target.value)} rows={8} placeholder="Write your note..." />
      <p className="hint">{text.length} / {MAX_TEXT}</p>
      {error && <p role="alert" className="error">{error}</p>}
      <button disabled={busy} onClick={save}>{busy ? "Analyzing..." : "Analyze and save"}</button>
    </section>
  );
}

function List({ onOpen }: { onOpen: (n: Note) => void }) {
  const [q, setQ] = useState("");
  const [page, setPage] = useState(1);
  const [notes, setNotes] = useState<Note[]>([]);
  const [error, setError] = useState<string | null>(null);

  // Debounce so every keystroke does not hit the API.
  useEffect(() => {
    const t = setTimeout(() => { listNotes(q, page).then(setNotes).catch((e) => setError(e.message)); }, 300);
    return () => clearTimeout(t);
  }, [q, page]);

  return (
    <section>
      <input value={q} onChange={(e) => { setQ(e.target.value); setPage(1); }} placeholder="Search notes by the start of a word" />
      {error && <p role="alert" className="error">{error}</p>}
      {notes.length === 0 && !error && <p className="hint">No notes found. Write one to get started.</p>}
      {notes.map((n) => (
        <article key={n.id} className="clickable" onClick={() => onOpen(n)}>
          <strong>{n.title}</strong>
          <span className="hint"> {new Date(n.createdAt).toLocaleDateString()}</span>
          <p className="hint">{n.analysis?.summary.slice(0, 120)}</p>
        </article>
      ))}
      <div className="row">
        <button className="ghost" disabled={page === 1} onClick={() => setPage(page - 1)}>Previous</button>
        <span className="hint">Page {page}</span>
        <button className="ghost" disabled={notes.length < 5} onClick={() => setPage(page + 1)}>Next</button>
      </div>
    </section>
  );
}

function Detail({ note, onBack }: { note: Note; onBack: () => void }) {
  const a = note.analysis;
  async function remove() { await deleteNote(note.id); onBack(); }
  return (
    <section>
      <button className="ghost" onClick={onBack}>Back</button>
      <h2>{note.title}</h2>
      {a && (
        <p>
          <span className={a.sentiment.label === "POSITIVE" ? "badge good" : "badge bad"}>
            {a.sentiment.label.toLowerCase()} ({Math.round(a.sentiment.score * 100)}%)
          </span>
        </p>
      )}
      {a && <p><strong>Summary:</strong> {a.summary}</p>}
      {a && <p><strong>Keywords:</strong> {a.keywords.join(", ")}</p>}
      <article><pre>{note.text}</pre></article>
      <button className="ghost" onClick={remove}>Delete note</button>
    </section>
  );
}
