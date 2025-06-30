// export const getTimezoneOffsetString = () => {
//   const offset = new Date().getTimezoneOffset();
//   const sign = offset <= 0 ? "+" : "-";
//   const hours = Math.floor(Math.abs(offset) / 60)
//     .toString()
//     .padStart(2, "0");
//   const mins = (Math.abs(offset) % 60).toString().padStart(2, "0");

//   return `${sign}${hours}:${mins}`;
// };

export const getTimezoneOffsetString = () => {
  const part = new Intl.DateTimeFormat("nl-NL", {
    timeZoneName: "longOffset"
  })
    .formatToParts(new Date())
    .find((part) => part.type === "timeZoneName");

  return part?.value.replace("GMT", "") || "+00:00";
};
