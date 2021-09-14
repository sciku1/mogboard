class UniversalisApi
{
    get(endpoint, queries, callback)
    {
        queries = queries ? queries : {};
        
        let query = Object.keys(queries)
            .map(k => encodeURIComponent(k) + '=' + encodeURIComponent(queries[k]))
            .join('&');

        endpoint = endpoint +'?'+ query;

        fetch(`https://universalis.app${endpoint}`)
            .then(response => response.json())
            .then(callback)
    }
}