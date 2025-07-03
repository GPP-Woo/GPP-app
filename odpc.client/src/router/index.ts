import { createRouter, createWebHistory } from "vue-router";
import LoginView from "@/views/LoginView.vue";
import ForbiddenView from "@/views/ForbiddenView.vue";
import PublicatiesView from "@/views/PublicatiesView.vue";

const resetFocus = () => {
  document.body.setAttribute("tabindex", "-1");
  document.body.focus();
  document.body.removeAttribute("tabindex");
};

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  scrollBehavior(to, from) {
    if (to.path === from.path) {
      return false;
    }

    resetFocus();

    return { top: 0 };
  },
  routes: [
    {
      path: "/",
      redirect: { name: "publicaties" }
    },
    {
      path: "/login",
      name: "login",
      component: LoginView,
      meta: {
        title: "Login"
      }
    },
    {
      path: "/forbidden",
      name: "forbidden",
      component: ForbiddenView,
      meta: {
        title: "Geen toegang",
        requiresAuth: true
      }
    },
    {
      path: "/publicaties/overzicht",
      name: "publicaties",
      component: PublicatiesView,
      meta: {
        title: "Publicaties",
        requiresAuth: true
      }
    },
    {
      path: "/publicaties/:uuid?",
      name: "publicatie",
      component: () => import("../views/PublicatieView.vue"),
      props: true,
      meta: {
        title: "Publicatie",
        requiresAuth: true
      }
    },
    {
      path: "/beheer/gebruikersgroepen/overzicht",
      name: "gebruikersgroepen",
      component: () => import("../views/beheer/GebruikersgroepenView.vue"),
      meta: {
        title: "Gebruikersgroepen",
        requiresAdmin: true
      }
    },
    {
      path: "/beheer/gebruikersgroepen/:uuid?",
      name: "gebruikersgroep",
      component: () => import("../views/beheer/GebruikersgroepView.vue"),
      props: true,
      meta: {
        title: "Gebruikersgroep",
        requiresAdmin: true
      }
    }
  ]
});

export default router;
