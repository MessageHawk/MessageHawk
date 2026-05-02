import { useCallback, useEffect, useState } from "react";
import "./App.css";

type InterchangeSummary = {
  id: string;
  messageTypeCode: string;
  updatedAt: string;
  currentStatus: string | null;
};

type LogStep = {
  id: string;
  sequenceNumber: number;
  sender: string;
  receiver: string;
  status: string;
  occurredAt: string;
  contentType: string | null;
  bodyBase64: string | null;
  indexedPropertiesJson: string | null;
};

type InterchangeDetail = {
  id: string;
  messageTypeCode: string;
  messageTypeDisplayName: string;
  createdAt: string;
  updatedAt: string;
  currentStatus: string | null;
  steps: LogStep[];
};

async function api<T>(path: string): Promise<T> {
  const res = await fetch(path, { credentials: "include" });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`${res.status} ${res.statusText}: ${text}`);
  }
  return (await res.json()) as T;
}

export default function App() {
  const [summaries, setSummaries] = useState<InterchangeSummary[]>([]);
  const [selected, setSelected] = useState<InterchangeDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const loadList = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const rows = await api<InterchangeSummary[]>("/api/v1/interchanges?limit=50");
      setSummaries(rows);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Request failed");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadList();
  }, [loadList]);

  const openDetail = async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const detail = await api<InterchangeDetail>(`/api/v1/interchanges/${id}`);
      setSelected(detail);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Request failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="layout">
      <header className="header">
        <h1>MessageHawk</h1>
        <p className="tagline">ESB integration message timeline</p>
        <button type="button" className="refresh" onClick={() => void loadList()} disabled={loading}>
          Refresh list
        </button>
      </header>

      {error ? <div className="banner error">{error}</div> : null}

      <div className="panels">
        <section className="panel">
          <h2>Recent interchanges</h2>
          {summaries.length === 0 && !loading ? (
            <p className="muted">No interchanges yet. POST steps to the ingest API to see data.</p>
          ) : (
            <ul className="list">
              {summaries.map((s) => (
                <li key={s.id}>
                  <button type="button" className="linkish" onClick={() => void openDetail(s.id)}>
                    {s.id}
                  </button>
                  <div className="meta">
                    <span>{s.messageTypeCode}</span>
                    {s.currentStatus ? <span className="pill">{s.currentStatus}</span> : null}
                    <span className="muted">{new Date(s.updatedAt).toLocaleString()}</span>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="panel detail">
          <h2>Detail</h2>
          {!selected ? (
            <p className="muted">Select an interchange to view steps and payloads.</p>
          ) : (
            <>
              <div className="detail-head">
                <div>
                  <div className="mono">{selected.id}</div>
                  <div>
                    {selected.messageTypeDisplayName} ({selected.messageTypeCode})
                  </div>
                  {selected.currentStatus ? <span className="pill">{selected.currentStatus}</span> : null}
                </div>
              </div>
              <ol className="steps">
                {selected.steps.map((step) => (
                  <li key={step.id} className="step">
                    <div className="step-head">
                      <strong>
                        #{step.sequenceNumber} — {step.status}
                      </strong>
                      <span className="muted">{new Date(step.occurredAt).toLocaleString()}</span>
                    </div>
                    <div className="step-route">
                      {step.sender} → {step.receiver}
                    </div>
                    {step.contentType ? <div className="muted small">Content-Type: {step.contentType}</div> : null}
                    {step.indexedPropertiesJson ? (
                      <pre className="json">{step.indexedPropertiesJson}</pre>
                    ) : null}
                    {step.bodyBase64 ? (
                      <pre className="body">{atob(step.bodyBase64)}</pre>
                    ) : (
                      <p className="muted small">No body stored for this step.</p>
                    )}
                  </li>
                ))}
              </ol>
            </>
          )}
        </section>
      </div>
    </div>
  );
}
