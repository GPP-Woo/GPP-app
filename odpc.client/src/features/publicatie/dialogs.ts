import type { DialogConfig } from "@/composables/use-multiple-confirm-dialogs";

export const Dialogs = {
  draftPublicatie: {
    message: `
        <span>Als je deze publicatie opslaat als concept, dan wordt deze <strong>niet</strong> online gepubliceerd.
        Je kan hem wel later nog aanpassen en alsnog publiceren. Wil je doorgaan?</span>
      `,
    cancelText: "Nee, keer terug",
    confirmText: "Ja, sla op als concept"
  },
  deletePublicatie: {
    message: `
        <span>Weet je zeker dat je deze publicatie inclusief documenten permanent wilt verwijderen?</span>
        <span><strong>Let op:</strong> deze actie kan niet ongedaan worden gemaakt.</span>
      `,
    cancelText: "Nee, keer terug",
    confirmText: "Ja, verwijderen"
  },
  retractPublicatie: {
    message: `
        <span>Weet je zeker dat je deze publicatie wilt intrekken?</span>
        <span><strong>Let op:</strong> deze actie kan niet ongedaan worden gemaakt.</span>
      `,
    cancelText: "Nee, gepubliceerd laten",
    confirmText: "Ja, intrekken"
  },
  noDocumenten: {
    message:
      "Aan deze publicatie zijn geen documenten toegevoegd. Weet je zeker dat je deze registratie wil publiceren?",
    cancelText: "Nee, documenten toevoegen",
    confirmText: "Ja, publiceren"
  }
} as const satisfies Record<string, DialogConfig>;
