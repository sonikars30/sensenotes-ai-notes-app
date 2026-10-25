const BASE = import.meta.env.VITE_API ?? "http://localhost:5000/api";

export const MAX_TITLE = 100;
export const MAX_TEXT = 5000;

export interface Note {
  id: string;
  title: string;
  text: string;
  createdAt: string;
  analysis?: { sentiment: { label: string; score: number }; summary: string; keywords: string[] };
}

async function parse<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error((await res.text()) || "Request failed.");
  return res.status === 204 ? (undefined as T) : res.json();
}

export const createNote = (title: string, text: string) =>
  fetch(`${BASE}/notes`, {
    method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ title, text }),
  }).then((r) => parse<Note>(r));

export const listNotes = (q: string, page: number) =>
  fetch(`${BASE}/notes?q=${encodeURIComponent(q)}&page=${page}&size=5`).then((r) => parse<Note[]>(r));

export const deleteNote = (id: string) => fetch(`${BASE}/notes/${id}`, { method: "DELETE" }).then((r) => parse<void>(r));
