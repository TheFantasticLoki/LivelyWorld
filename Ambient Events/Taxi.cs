using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lively_World
{
    public class TaxiEvent
    {
        Ped hitch;
        Blip hitchBlip;
        Ped Driver;
        Vehicle Taxi;
        Blip TaxiBlip;
       public bool Finished = false;
        int Status = 0;
        float Range = 300f;
        bool DropOff = false;
        public TaxiEvent(Ped ped, Ped driver, Vehicle taxi)
        {
            hitch = ped;
            Driver = driver;
            Taxi = taxi;

            hitch.IsPersistent = true;            
            Taxi.IsPersistent = true;
            Driver.IsPersistent = true;
            if (LivelyWorld.DebugBlips && !TaxiBlip.Exists())
            {
                TaxiBlip = Taxi.AddBlip();
                TaxiBlip.Sprite = BlipSprite.Cab;
                TaxiBlip.Color = BlipColor.Yellow;
                TaxiBlip.IsShortRange = true;

                hitchBlip = hitch.AddBlip();
                hitchBlip.Color = BlipColor.White;
                hitchBlip.Scale = 0.7f;
                hitchBlip.IsShortRange = true;
                
            }


            hitch.AlwaysKeepTask = true;
            driver.AlwaysKeepTask = true;

            if (hitch.IsInVehicle(Taxi))
            {
               if(LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) GTA.UI.Notification.Show("Taxi event mode: dropoff");
                DropOff = true;
                 //Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, Driver, Taxi, pos.X, pos.Y, pos.Z, 10f, 1, Taxi.Model, 1 + 2 + 8 + 16 + 32 + 128 + 256, 15.0, 1.0);
            }
            else
            {
                if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) GTA.UI.Notification.Show("Taxi event mode: pick up");

                Vector3 pos = hitch.Position;
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, Driver, Taxi, pos.X, pos.Y, pos.Z, 10f, 1, Taxi.Model, 1 + 2 + 8 + 16 + 32 + 128 + 256, 15.0, 1.0);
                hitch.Task.LookAt(Taxi, -1);
            }

            //Driver.Task.DriveTo(Taxi, hitch.Position, 10f, 10f, 1+2+ 16+32 + 128 + 256);
        }
        public void Process()
        {
            if (hitch.IsInCombat)
            {
                Finished = true;
                return;
            }
            if (LivelyWorld.CanWeUse(Taxi) && LivelyWorld.CanWeUse(hitch) && LivelyWorld.CanWeUse(Driver) && (Taxi.Position.DistanceTo(Game.Player.Character.Position) < Range))
            {
                if (DropOff)
                {
                    if (Status == 0)
                    {
                        Status++;
                        Vector3 pos = World.GetSafeCoordForPed(Taxi.Position + (Taxi.ForwardVector * 50f));

                        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, Driver, Taxi, pos.X, pos.Y, pos.Z, 10f, 1, Taxi.Model, 1 + 2 + 8 + 16 + 32 + 128 + 256, 5.0, 30.0);
                    }
                    else
                    {
                        if (Taxi.IsStopped)
                        {
                            hitch.Task.LeaveVehicle();
                            Finished = true;
                        }
                    }

                }
                else
                {
                    switch (Status)
                    {
                        case 0:
                            {
                                if ((Taxi.Position.DistanceTo(hitch.Position) < 20f))
                                {
                                    Status++;
                                    Vector3 pos = Taxi.Position + (Taxi.ForwardVector * 10f) + (Taxi.RightVector * 2);
                                    Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, Driver, Taxi, pos.X, pos.Y, pos.Z, 3f, 1, Taxi.Model, 4 + 8 + 16 + 32, 3.0, 50.0);
                                }
                                else if (Taxi.IsStopped)
                                {
                                    Vector3 pos = hitch.Position;
                                    Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, Driver, Taxi, pos.X, pos.Y, pos.Z, 10f, 1, Taxi.Model, 1 + 2 + 8 + 16 + 32 + 128 + 256, 15.0, 1.0);
                                    //Driver.Task.DriveTo(Taxi, hitch.Position, 10f, 10f, 1 + 2  + 16 + 32+ 128 + 256);
                                }
                                break;
                            }
                        case 1:
                            {
                                if (hitch.IsInVehicle(Taxi))
                                {
                                    Status++;
                                    Range = 50;
                                }
                                else if (!hitch.IsInVehicle() && hitch.TaskSequenceProgress == -1)
                                {
                                    hitch.Task.EnterVehicle(Taxi, VehicleSeat.RightRear, -1, 1f);
                                }
                                break;
                            }
                        case 2:
                            {
                                if (Taxi.IsStopped) Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, Driver, Taxi, 20f, 1 + 2 + 4 + 8 + 16 + 32 + 128 + 256);
                                break;
                            }
                    }
                }
            }
            else if (!Finished) Finished = true;
        }
        public void Clear()
        {
            if (TaxiBlip.Exists()) TaxiBlip.Color = BlipColor.White;

            //if (hitch.CurrentBlip.Exists())  hitch.CurrentBlip.Remove();
            //if (Taxi.CurrentBlip.Exists()) Taxi.CurrentBlip.Remove();
            hitch.IsPersistent = false;
            Taxi.IsPersistent = false;
            Driver.IsPersistent = false;
        }
    }
}
