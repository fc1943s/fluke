import * as Gun from 'gun';


require('gun/sea');
require('gun/lib/promise');

interface AppState {
    users: { username: string }[],
    tasks: {
        taskId?: string
        name?: string,
        cells?: { dateId: string, status?: string, selected?: boolean }[]
    }[],
    trees: { position: string }[],
    taskId1: {}
}

if(process.env.JEST_WORKER_ID) {
    const gun = Gun<AppState>(["http://localhost:8765/gun"]);
    window['gun'] = gun;
}
const gun = window['gun'];

window['login'] = async (username = 'fc1943s', password = 'pw1') => {
    const user = gun.user();
    console.log('user', user);
    // @ts-ignore
    const userData = await gun.get('~@' + username).promOnce();

    if (userData) {
        console.log('user already exists', userData);
    }
}

window['A'] = async () => {
    const tasks = gun.get("tasks");
    const task1 = gun.get("taskId1").put({id: 'taskId1', name: 'taskName1'});
    tasks.set(task1);

    const cells = task1.get("cells");
    const cell1 = gun.get("dateId1").put({dateId: 'dateId1'});
    const cell = cells.set(cell1);

    cell.put({selected: true});
//                                    let task = tasks.set ({| taskId = string _taskId |})
//                                    let cells = task.get "cells"
//                                    let cell = cells.set ({| dateId = string _dateId |})
//                                    cell.put {| selected = value |} |> ignore


    return;

    // const users = gun.get("users");
    // const testUser = users.set({username: "fluke"});
    //
    // const task1 = tasks.set({name: '01'});
    // const cells1 = task1.get("cells");
    // const cell1 = cells1.set({dateId: '2020-03-09'});
    // const y = cell1.put({status: 'Disabled'});
    //
    // const task2 = tasks.set({name: '02'});
    // const cells2 = task2.get("cells");
    // const cell2 = cells2.set({dateId: '2020-03-09'});
    // const x = cell2.put({status: 'Postponed None'});
    //
    // const event = {selected: true};
    //
    // const trees = gun.get("trees");
    // trees.set({position: '2020-03-10 14:00'});
}
