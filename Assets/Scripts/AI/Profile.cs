﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este struct clasifica y separa las funciones de conveniencia de stats y las funciones de conveniencia de órdenes
public struct ProfileInfo {

    #region Profile info type
    public enum Type {
        STAT, ORDER
    }

    public Type type;
    #endregion

    public Stat stat;

    public Order order;

    public System.Func<Architect, WorldState, float> statConvenience;

    public System.Func<Actionable, Actionable, WorldState, float> orderConvenience;

    public ProfileInfo(Stat stat, System.Func<Architect, WorldState, float> statConvenience) {
        this.type = ProfileInfo.Type.STAT;
        this.stat = stat;
        this.statConvenience = statConvenience;
        this.order = null;
        this.orderConvenience = null;
    }

    public ProfileInfo(Order order, System.Func<Actionable, Actionable, WorldState, float> orderConvenience) {
        this.type = ProfileInfo.Type.ORDER;
        this.order = order;
        this.orderConvenience = orderConvenience;
        this.stat = null;
        this.statConvenience = null;
    }
}

// Los perfiles asignan funciones de conveniencia a los stats y reflejan formas de jugar
public class Profile {

   private Dictionary<Stat, System.Func<Architect, WorldState, float>> statConvenience;

   private Dictionary<Order, System.Func<Actionable, Actionable, WorldState, float>> orderConvenience;

    public Profile(ProfileInfo[] info) {
        this.statConvenience = new Dictionary<Stat, System.Func<Architect, WorldState, float>>();
        this.orderConvenience = new Dictionary<Order, System.Func<Actionable, Actionable, WorldState, float>>();

        foreach(var profileinfo in info) {
            
            switch(profileinfo.type) {
                case ProfileInfo.Type.STAT:
                    this.statConvenience[profileinfo.stat] = profileinfo.statConvenience;
                    break;
                case ProfileInfo.Type.ORDER:
                    this.orderConvenience[profileinfo.order] = profileinfo.orderConvenience;
                    break;
            }
        }

        Profiles.RegisterProfile(this);
    }

    public float GetConvenienceFor(Architect architect, Stat stat, WorldState state) {
        if(!this.statConvenience.ContainsKey(stat)) {
            throw new System.Exception("Este perfil no tiene una función de conveniencia para el stat indicado.");
        }
        return this.statConvenience[stat](architect, state);
    }

    public float GetConvenienceFor(Actionable actionable, Order order, Actionable target, WorldState state) {
        if(!this.orderConvenience.ContainsKey(order)) {
            throw new System.Exception("Este perfil no tiene una función de conveniencia para la orden indicada.");
        }
        return this.orderConvenience[order](actionable, target, state);
    }

    public float GetConvenienceFor(WorldState state) {
        var ret = 0f;
        foreach(var architect in state.Architects.Keys) {
            foreach(var stat in Stats.GetAll()) {
                ret += this.GetConvenienceFor(architect, stat, state);
            }
        }
        return ret;
    }

    public static explicit operator Profile(Profiles.Enum enumValue) {
        return Profiles.GetAll()[(int) enumValue];
    }
}

public class Profiles {

    // Perfil predeterminado y único disponible por ahora

    public static readonly Profile DEFAULT = new Profile(new ProfileInfo[] {
        
        new ProfileInfo(Stats.SIZE, (self, worldState) => {
            var ret = 0;
            foreach(var architect in worldState.Architects.Keys) {

                // Valoramos más tener casillas propias que que los enemigos las tengan
                if(architect == self) {
                    ret += 5 * worldState[architect][Stats.SIZE];
                } else {
                    ret -= 1 * worldState[architect][Stats.SIZE];
                }
            }
            return ret;
        }),
        
        new ProfileInfo(Stats.FULL_ARMY_AMOUNT, (self, worldState) => {
            var ret = 0;
            foreach(var architect in worldState.Architects.Keys) {

                // Que los enemigos tengan muchas tropas es mucho menos conveniente que que nosotros las tengamos
                if(architect == self) {
                    ret -= 1 * worldState[architect][Stats.FULL_ARMY_AMOUNT];
                } else {
                    ret -= 8 * worldState[architect][Stats.FULL_ARMY_AMOUNT];
                }
            }
            return ret;
        }),

        new ProfileInfo(Stats.SPADE_AMOUNT, (self, worldState) => {
            var ret = 0;
            foreach(var architect in worldState.Architects.Keys) {
                // Esto solo es poco conveniente si nuestros ejércitos son débiles
                if(architect != self) {
                    ret -= 1 * worldState[architect][Stats.HEART_AMOUNT];
                }
            }
            return ret;
        }),

        new ProfileInfo(Stats.HEART_AMOUNT, (self, worldState) => {
            var ret = 0;
            foreach(var architect in worldState.Architects.Keys) {
                // Esto solo es poco conveniente si nuestros ejércitos son débiles
                if(architect != self) {
                    ret -= 1 * worldState[architect][Stats.CLUB_AMOUNT];
                }
            }
            return ret;
        }),

        new ProfileInfo(Stats.CLUB_AMOUNT, (self, worldState) => {
            var ret = 0;
            foreach(var architect in worldState.Architects.Keys) {
                // Esto solo es poco conveniente si nuestros ejércitos son débiles
                if(architect != self) {
                    ret -= 1 * worldState[architect][Stats.DIAMOND_AMOUNT];
                }
            }
            return ret;
        }),

        new ProfileInfo(Stats.DIAMOND_AMOUNT, (self, worldState) => {
            var ret = 0;
            foreach(var architect in worldState.Architects.Keys) {
                // Esto solo es poco conveniente si nuestros ejércitos son débiles
                if(architect != self) {
                    ret -= 1 * worldState[architect][Stats.SPADE_AMOUNT];
                }
            }
            return ret;
        }),

        new ProfileInfo(Orders.MOVE_TOWARDS, (subject, target, worldState) => {
            // No tener ejércitos vuelve esta orden completamente inviable
            if(worldState[((Army) subject).Nation.Architect][Stats.FULL_ARMY_AMOUNT] <= 0) {
                return -Mathf.Infinity;
            }

            // Si ya estamos en la casilla que deberíamos, no es conveniente ni inconveniente
            var distance = subject.RequestDistanceFrom(target);
            if(distance == 0) {
                return 0;

            // Si no, cuanto más lejos esté menos conveniente es
            } else {
                return -10 * subject.RequestDistanceFrom(target);
            }
        }),

        new ProfileInfo(Orders.ATTACK, (subject, target, worldState) => {
            // No tener ejércitos vuelve esta orden completamente inviable
            if(worldState[((Army) subject).Nation.Architect][Stats.FULL_ARMY_AMOUNT] <= 0) {
                return -Mathf.Infinity;
            }

            // Atacar a un ejército contra el que somos débiles es menos conveniente
            var ret = 0;
            if(!((Army) target).Suit.IsWeakAgainst(((Army) subject).Suit)) {
                ret = -2;
            }
            return ret;
        }),

        new ProfileInfo(Orders.CLAIM, (subject, target, worldState) => {
            // No tener ejércitos vuelve esta orden completamente inviable
            if(worldState[((Army) subject).Nation.Architect][Stats.FULL_ARMY_AMOUNT] <= 0) {
                return -Mathf.Infinity;
            }

            // Si la casilla ya está conquistada, esta orden es una pérdida de tiempo
            if(((Cell) target).Nation == ((Army) subject).Nation) {
                return -Mathf.Infinity;
            }

            // Conquistar una casilla es tan inconveniente como lejos esté
            var distance = subject.RequestDistanceFrom(target);
            if(distance == 0 || worldState[((Army) subject).Nation.Architect][Stats.SIZE] == 0) {
                return 0;
            }
            return -2 * subject.RequestDistanceFrom(target);
        }),

        new ProfileInfo(Orders.BUILD_SPADE, (cell, target, worldState) => {
            var architect = ((Cell) cell).Nation.Architect;

            // Si ya hemos llegado al tope de tropas que podemos tener por cada casilla, no se pueden generar más
            if(worldState[architect][Stats.FULL_ARMY_AMOUNT] < worldState[architect][Stats.SIZE]) {
                // Si ya hay un ejército en esa casilla no podemos construir más
                if(((Cell) cell).Army != null) {
                    return -Mathf.Infinity;
                } else {
                    return 0;
                }
            } else {
                return -Mathf.Infinity;
            }
        }),

        new ProfileInfo(Orders.BUILD_HEART, (cell, target, worldState) => {
            var architect = ((Cell) cell).Nation.Architect;

            // Si ya hemos llegado al tope de tropas que podemos tener por cada casilla, no se pueden generar más
            if(worldState[architect][Stats.FULL_ARMY_AMOUNT] < worldState[architect][Stats.SIZE]) {
                return 0;
            } else {
                return -Mathf.Infinity;
            }
        }),

        new ProfileInfo(Orders.BUILD_CLUB, (cell, target, worldState) => {
            var architect = ((Cell) cell).Nation.Architect;

            // Si ya hemos llegado al tope de tropas que podemos tener por cada casilla, no se pueden generar más
            if(worldState[architect][Stats.FULL_ARMY_AMOUNT] < worldState[architect][Stats.SIZE]) {
                return 0;
            } else {
                return -Mathf.Infinity;
            }
        }),

        new ProfileInfo(Orders.BUILD_DIAMOND, (cell, target, worldState) => {
            var architect = ((Cell) cell).Nation.Architect;

            // Si ya hemos llegado al tope de tropas que podemos tener por cada casilla, no se pueden generar más
            if(worldState[architect][Stats.FULL_ARMY_AMOUNT] < worldState[architect][Stats.SIZE]) {
                return 0;
            } else {
                return -Mathf.Infinity;
            }
        })
    });

    #region Permitir iterar perfiles
    private static List<Profile> profiles;
    public static void RegisterProfile(Profile stat) {
        if(Profiles.profiles == null) {
            Profiles.profiles = new List<Profile>();
        }
        Profiles.profiles.Add(stat);
    }

    public static List<Profile> GetAll() {
        return Profiles.profiles;
    }

    // Este enumerado permite acceder a los perfiles mediante un menú desplegable en el inspector
    public enum Enum {
        DEFAULT
    }
    #endregion
}