import toast from "@/stores/toast";

export const handleFetchError = (status?: number) => {
  switch (status) {
    case 401:
      toast.add({
        text: `
          U bent niet meer aangemeld. Volg onderstaande stappen om weer in te loggen, zonder ingevulde gegevens kwijt te raken.

          <ol>
            <li>Gebruik <a href="/" target="_blank">deze link</a> of open zelf een nieuw tabblad om de applicatie opnieuw te starten en in te loggen</li>
            <li>U kunt daarna het tabblad waarop u opnieuw ingelogd bent weer sluiten</li>
            <li>En dan kunt u in dit venster verder werken</li>
          </ol>
        `,
        type: "error"
      });

      break;
    default:
      break;
  }
};
