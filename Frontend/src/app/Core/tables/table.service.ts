import { Layout } from './table.model';

const layouts:Layout[] = [{
    key: 0,
    name: 'Layout0',
}, {
    key: 1,
    name: 'Layout1',
}, {
    key: 2,
    name: 'Layout2',
}];
export class TableService {
    getLayouts(){
        return layouts
    }
}
