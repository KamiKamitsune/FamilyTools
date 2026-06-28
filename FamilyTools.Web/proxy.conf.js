const easyComptaTarget =
  process.env["services__easycomptaapi__https__0"] ||
  process.env["services__easycomptaapi__http__0"];

const accountsTarget =
  process.env["services__accounts__https__0"] ||
  process.env["services__accounts__http__0"];

const secure = process.env["NODE_ENV"] !== "development";

module.exports = {
  // API REST EasyCompta
  "/api": {
    target: easyComptaTarget,
    secure,
    pathRewrite: {
      "^/api": "",
    },
  },
  // Hub SignalR (notifications de fin d'import) — WebSocket activé.
  "/hubs": {
    target: easyComptaTarget,
    secure,
    ws: true,
  },
  // API transversale "Mon compte" (Accounts). Prefixe distinct de la route SPA /account
  // pour ne pas intercepter la navigation Angular ; reecrit vers le groupe serveur /account.
  "/identity": {
    target: accountsTarget,
    secure,
    pathRewrite: {
      "^/identity": "",
    },
  },
};
