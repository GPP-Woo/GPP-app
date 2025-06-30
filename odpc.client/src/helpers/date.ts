export const getTimezoneOffsetString = () => {
  const part = new Intl.DateTimeFormat("nl-NL", {
    timeZoneName: "longOffset"
  })
    .formatToParts(new Date())
    .find((part) => part.type === "timeZoneName");

  return part?.value.replace("GMT", "") || "+00:00";
};


export const ISOToday = new Date().toISOString().split("T")[0];