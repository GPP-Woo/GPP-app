import toast from "@/stores/toast";

export const handleFetchError = (status?: number) => {
  switch (status) {
    case 401:
      toast.add({
        text: `
          U bent niet meer aangemeld. Volg onderstaande stappen om weer in te loggen, zonder ingevulde gegevens kwijt te raken.

          <ol>
            <li>Gebruik <a href="/" target="_blank">deze link</a> (of open zelf een nieuw tabblad) om de applicatie opnieuw te starten en in te loggen</li>
            <li>Daarna kunt u dat tabblad weer sluiten en in dit scherm verder werken</li>
          </ol>
        `,
        type: "error"
      });

      throw new Error(`Logged out: 401`);
    default:
      break;
  }
};
