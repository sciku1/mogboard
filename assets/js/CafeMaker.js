import XIVAPI from "./XIVAPI";

class CafeMaker extends XIVAPI
{
    get(endpoint, queries, callback)
    {
        queries = queries ? queries : {};
        
        let query = Object.keys(queries)
            .map(k => encodeURIComponent(k) + '=' + encodeURIComponent(queries[k]))
            .join('&');

        endpoint = endpoint +'?'+ query;

        fetch(`https://cafemaker.wakingsands.com${endpoint}`, { mode: 'cors' })
            .then(response => response.json())
            .then(callback)
    }
}

export default new CafeMaker;
