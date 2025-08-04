type DateLike = string | null | undefined | Date;

const nlLongFormat = Intl.DateTimeFormat("nl-NL", { dateStyle: "long" });

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

  const year = date.getFullYear().toString().padStart(4, "0"),
    month = (date.getMonth() + 1).toString().padStart(2, "0"),
    day = date.getDate().toString().padStart(2, "0");

  return [year, month, day].join("-");
};

export const ISOToday = formatIsoDate(new Date());
