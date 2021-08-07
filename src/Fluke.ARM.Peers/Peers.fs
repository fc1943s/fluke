namespace Fluke.ARM.GunPeers

open System
open System.IO
open Farmer
open Farmer.Builders

module Peers =
    module Storage =
        type FileShareId = FileShareId of id: string

        let rec peersstorage1 (FileShareId fileShareId) =
            storageAccount {
                name (nameof peersstorage1)
                sku Storage.Sku.Standard_LRS
                add_file_share_with_quota fileShareId 5<Gb>
            }

    type Port = Port of int
    type ContainerId = ContainerId of id: string

    module Gun =
        let serverPort = Port 8765

        let rec gunPeer (ContainerId containerId) (Storage.FileShareId fileShareId) =
            let rec ``share-fluke-gun-peer`` = nameof ``share-fluke-gun-peer``

            containerGroup {
                restart_policy ContainerGroup.AlwaysRestart
                name containerId

                public_dns
                    containerId
                    [
                        let (Port port) = serverPort
                        TCP, uint16 port
                    ]

                add_volumes [
                    volume_mount.secrets
                        ``share-fluke-gun-peer``
                        [
                            let projectDir = "Fluke.GunPeer"
                            "package.json", (File.ReadAllBytes $"../{projectDir}/package.json")
                            "yarn.lock", (File.ReadAllBytes $"../{projectDir}/yarn.lock")
                            "server.js", (File.ReadAllBytes $"../{projectDir}/server.js")

                            "cert.pem",
                            (File.ReadAllBytes $"../{projectDir}/ssl/{containerId}.eastus.azurecontainer.io.pem")

                            "key.pem",
                            (File.ReadAllBytes $"../{projectDir}/ssl/{containerId}.eastus.azurecontainer.io-key.pem")
                        ]
                    volume_mount.azureFile
                        fileShareId
                        fileShareId
                        (Storage.peersstorage1 (Storage.FileShareId fileShareId))
                            .Name
                            .ResourceName
                            .Value
                ]

                add_instances [
                    containerInstance {
                        name (nameof containerInstance)
                        image "node:16.5-alpine"

                        env_vars [
                            "GUN_FILE", $"/data/{fileShareId}/{containerId}-radata"
                            "HTTPS_KEY", "/app/key.pem"
                            "HTTPS_CERT", "/app/cert.pem"
                        ]

                        add_public_ports [
                            let (Port port) = serverPort
                            uint16 port
                        ]

                        cpu_cores 1
                        memory 0.2<Gb>
                        add_volume_mount fileShareId $"/data/{fileShareId}"
                        add_volume_mount ``share-fluke-gun-peer`` "/app"

                        command_line [
                            "/bin/sh"
                            "-c"
                            [
                                "cd /app"
                                "apk add --no-cache git"
                                "yarn install"
                                "while true; do yarn start && break"
                                "sleep 30"
                                "done"
                            ]
                            |> String.concat "; "
                        ]
                    }
                ]
            }

    module Hub =
        let serverPort = Port 33929

        let rec hubPeer (ContainerId containerId) (Storage.FileShareId fileShareId) =
            let rec ``share-fluke-hub-peer`` = nameof ``share-fluke-hub-peer``

            containerGroup {
                restart_policy ContainerGroup.AlwaysRestart
                name containerId

                public_dns
                    containerId
                    [
                        let (Port port) = serverPort
                        TCP, uint16 port
                    ]

                add_volumes [
                    volume_mount.secrets
                        ``share-fluke-hub-peer``
                        [
                            let projectDir = "Fluke.UI.Backend"

                            "cert.pem",
                            (File.ReadAllBytes $"../{projectDir}/ssl/{containerId}.eastus.azurecontainer.io.pem")

                            "key.pem",
                            (File.ReadAllBytes $"../{projectDir}/ssl/{containerId}.eastus.azurecontainer.io-key.pem")
                        ]
                    volume_mount.azureFile
                        fileShareId
                        fileShareId
                        (Storage.peersstorage1 (Storage.FileShareId fileShareId))
                            .Name
                            .ResourceName
                            .Value
                ]

                add_instances [
                    containerInstance {
                        name (nameof containerInstance)
                        image "mcr.microsoft.com/dotnet/sdk:6.0-alpine"

                        env_vars [
                            "DATA_PATH", $"/data/{fileShareId}/{containerId}-hubdata"
                            "HTTPS_KEY", "/app/key.pem"
                            "HTTPS_CERT", "/app/cert.pem"
                        ]

                        add_public_ports [
                            let (Port port) = serverPort
                            uint16 port
                        ]

                        cpu_cores 1
                        memory 0.2<Gb>
                        add_volume_mount fileShareId $"/data/{fileShareId}"
                        add_volume_mount ``share-fluke-hub-peer`` "/app"


                        command_line [
                            "/bin/sh"
                            "-c"
                            [
                                "cd /app"
                                "apk add --no-cache git"
                                "git clone https://github.com/fc1943s/fluke.git"
                                "cd fluke/src/Fluke.UI.Backend"
                                "dotnet tool restore"
                                "dotnet paket restore"
                                "while true; do dotnet run && break"
                                "sleep 30"
                                "done"
                            ]
                            |> String.concat "; "
                        ]
                    }
                ]
            }


    let deployment =
        let fileShareId = Storage.FileShareId $"share-{nameof Storage.peersstorage1}"

        arm {
            location Location.EastUS

            add_resources [
                Storage.peersstorage1 fileShareId
                //                deploymentScript1 shareName
                Gun.gunPeer
                    (ContainerId (Environment.GetEnvironmentVariable "FLUKE_GUN_PEER_CONTAINER_ID_1"))
                    fileShareId

                Hub.hubPeer
                    (ContainerId (Environment.GetEnvironmentVariable "FLUKE_HUB_PEER_CONTAINER_ID_1"))
                    fileShareId
                //                gunPeer (ContainerId "flukegunpeer-test") fileShareId
                ]
        }
