﻿/// <reference path="../excalibur-0.6.0.d.ts"/>
/// <reference path="../typings/lodash/lodash.d.ts"/>
/// <reference path="Assets.ts"/>
/// <reference path="GameSession.ts"/>

class GameSession {
   
   static Game: ex.Engine;
   static SessionId: number;
   static SessionState: Models.IGameSession;

   static create(sessionId: number) {
      GameSession.SessionId = sessionId;

      var game = new ex.Engine({
         canvasElementId: "game",
         height: 480,
         width: 720
      });

      // load assets
      var loader = new ex.Loader();      
      _.forIn(Assets, (a) => loader.addResource(a));

      game.start(loader).then(GameSession.init);

      GameSession.Game = game;
   }

   static init() {
      


   }

   static updateSessionState() {

      $.getJSON(`/api/games/${GameSession.SessionId}`).then(sess => {
         GameSession.SessionState = sess;
      });

   }
}