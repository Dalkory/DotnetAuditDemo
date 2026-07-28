const form = document.querySelector("#form");
const input = document.querySelector("#file");
const label = document.querySelector("#fileLabel");
const loading = document.querySelector("#loading");
const error = document.querySelector("#error");
const results = document.querySelector("#results");
let lastResult = null;

input.addEventListener("change", () => {
  label.textContent = input.files[0]?.name ?? "Выбрать XLSX или CSV";
});

form.addEventListener("submit", async event => {
  event.preventDefault();
  if (!input.files[0]) {
    showError("Сначала выберите файл.");
    return;
  }

  loading.classList.remove("hidden");
  error.classList.add("hidden");
  results.classList.add("hidden");
  const body = new FormData();
  body.append("file", input.files[0]);

  try {
    const response = await fetch("/api/analyze", { method: "POST", body });
    const payload = await response.json();
    if (!response.ok) throw new Error(payload.error ?? "Не удалось обработать файл.");
    lastResult = payload;
    render(payload);
  } catch (exception) {
    showError(exception.message);
  } finally {
    loading.classList.add("hidden");
  }
});

document.querySelector("#export").addEventListener("click", () => {
  if (!lastResult) return;
  const markdown = [
    "# AI Insights Report",
    "",
    lastResult.summary,
    "",
    "## Anomalies",
    ...lastResult.anomalies.map(value => `- ${value}`),
    "",
    "## Risks",
    ...lastResult.risks.map(value => `- ${value}`),
    "",
    "## Recommended questions",
    ...lastResult.recommendedQuestions.map(value => `- ${value}`),
    "",
    `Rows processed: ${lastResult.audit.rowsProcessed}`,
    `Rows shared externally: ${lastResult.audit.rowsSharedExternally}`,
    `Analysis version: ${lastResult.audit.analysisVersion}`
  ].join("\n");
  const link = document.createElement("a");
  link.href = URL.createObjectURL(new Blob([markdown], { type: "text/markdown" }));
  link.download = "ai-insights-report.md";
  link.click();
  URL.revokeObjectURL(link.href);
});

function render(data) {
  document.querySelector("#summary").textContent = data.summary;
  document.querySelector("#rows").textContent = data.audit.rowsProcessed.toLocaleString("ru-RU");
  document.querySelector("#external").textContent = data.audit.rowsSharedExternally;
  fillList("#anomalies", data.anomalies);
  fillList("#risks", data.risks);
  fillList("#questions", data.recommendedQuestions);
  document.querySelector("#departments").innerHTML = data.departmentMetrics.map(metric => `
    <tr>
      <td>${escapeHtml(metric.department)}</td>
      <td>${format(metric.plan)}</td>
      <td>${format(metric.actual)}</td>
      <td>${format(metric.attainmentPercent)}%</td>
      <td>${format(metric.variance)}</td>
    </tr>`).join("");
  results.classList.remove("hidden");
}

function fillList(selector, values) {
  document.querySelector(selector).innerHTML = values.map(value => `<li>${escapeHtml(value)}</li>`).join("");
}

function showError(message) {
  error.textContent = message;
  error.classList.remove("hidden");
}

function format(value) {
  return Number(value).toLocaleString("ru-RU", { maximumFractionDigits: 1 });
}

function escapeHtml(value) {
  const node = document.createElement("div");
  node.textContent = value;
  return node.innerHTML;
}

