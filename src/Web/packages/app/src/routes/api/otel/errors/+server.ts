import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { trace, SpanStatusCode } from "@opentelemetry/api";

const tracer = trace.getTracer("nocturne-web-client", "1.0.0");

export const POST: RequestHandler = async ({ request }) => {
  let body: {
    message: string;
    stack?: string;
    url: string;
    errorId: string;
    userAgent?: string;
  };

  try {
    body = await request.json();
  } catch {
    return json({ error: "Invalid JSON" }, { status: 400 });
  }

  const span = tracer.startSpan("client-error", {
    attributes: {
      "error.id": body.errorId,
      "error.message": body.message,
      "error.stack": body.stack ?? "",
      "error.url": body.url,
      "http.user_agent":
        body.userAgent ?? request.headers.get("user-agent") ?? "",
    },
  });

  span.setStatus({ code: SpanStatusCode.ERROR, message: body.message });
  span.end();

  return new Response(null, { status: 204 });
};
