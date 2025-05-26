import type { DialogConfig } from "@/composables/use-multiple-confirm-dialogs";

export const Dialogs = {
  noDocumenten: {
    message:
      "Aan deze publicatie zijn geen documenten toegevoegd. Weet je zeker dat je deze registratie wil publiceren?",
    cancelText: "Nee, documenten toevoegen",
    confirmText: "Ja, publiceren"
  },
  retractPublicatie: {
    message: `
        <span>Weet je zeker dat je deze publicatie wilt intrekken?</span>
        <span><strong>Let op:</strong> deze actie kan niet ongedaan worden gemaakt.</span>
      `,
    cancelText: "Nee, gepubliceerd laten",
    confirmText: "Ja, intrekken"
  }
} as const satisfies Record<string, DialogConfig>;
