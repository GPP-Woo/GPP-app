type DateLike = string | null | undefined | Date;

const NL_TIME_ZONE = "Europe/Amsterdam";

const nlLongFormat = Intl.DateTimeFormat("nl-NL", { dateStyle: "long", timeZone: NL_TIME_ZONE });

export const getTimezoneOffsetString = (date: DateLike) => {
  date = parseValidDate(date);
  if (!date) return undefined;

  const part = new Intl.DateTimeFormat("nl-NL", {
    timeZoneName: "longOffset"
  })
    .formatToParts(date)
    .find((part) => part.type === "timeZoneName");

  return part?.value.replace("GMT", "");
};

const parseValidDate = (date: DateLike) => {
  if (!date) return undefined;
  date = new Date(date);

  if (date instanceof Date && !isNaN(date.getTime())) return date;
  return undefined;
};

export const formatDate = (date: DateLike) => {
  date = parseValidDate(date);
  if (!date) return undefined;

  return nlLongFormat.format(date);
};

export const formatIsoDate = (date: DateLike) => {
  date = parseValidDate(date);
  if (!date) return undefined;

  const parts = new Intl.DateTimeFormat("nl-NL", {
    timeZone: NL_TIME_ZONE,
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  }).formatToParts(date);

  const get = (type: "year" | "month" | "day") => parts.find((p) => p.type === type)?.value;

  return [get("year"), get("month"), get("day")].join("-");
};

export const todayIsoDate = () => formatIsoDate(new Date())!;

export const addDays = (date: DateLike, days: number) => {
  const isoDate = formatIsoDate(date); // normalize to NL_TIME_ZONE
  if (!isoDate) return "";

  const [year, month, day] = isoDate.split("-").map(Number);

  const result = new Date(Date.UTC(year, month - 1, day + days));

  return result.toISOString().slice(0, 10);
};

export const tomorrowIsoDate = () => addDays(todayIsoDate(), 1);
