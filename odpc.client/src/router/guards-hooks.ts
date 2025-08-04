import type { App } from "vue";
import type { Router, RouteLocationNormalized } from "vue-router";
import getUser from "@/stores/user";
import { useAppData } from "@/composables/use-app-data";
import { setPreviousRoute } from "@/composables/use-previous-route";

async function authGuard(to: RouteLocationNormalized) {
  const requiresAuth = to.matched.some((route) => route.meta.requiresAuth);
  const requiresAdmin = to.matched.some((route) => route.meta.requiresAdmin);

  const user = await getUser(false);

  if ((requiresAuth || requiresAdmin) && !user?.isLoggedIn) {
    return { name: "login" };
  }

  if (requiresAdmin && !user?.isAdmin) {
    return { name: "forbidden" };
  }

  if (user?.isLoggedIn) await useAppData().fetchData();
}

function titleHook(to: RouteLocationNormalized) {
  document.title = `${to.meta?.title || ""} | ${import.meta.env.VITE_APP_TITLE}`;
}

function previousRouteHook(_: RouteLocationNormalized, from: RouteLocationNormalized) {
  setPreviousRoute(from);
}

export default {
  install(_: App, router: Router) {
    router.beforeEach(authGuard);
    router.beforeEach(titleHook);
    router.beforeEach(previousRouteHook);
  }
};
