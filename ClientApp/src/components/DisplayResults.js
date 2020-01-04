import React, { useState, useEffect } from 'react';

const ARTIST = 'ARTIST';
const ALBUMS = 'ALBUMS';
const TRACKS = 'TRACKS';

const parseTracks = ((results, title, format) => {
    let parseToJSX = (tracks, title, format) => {
        console.log('at parseToJSX');
        return tracks.map(row =>
            <tr key={row.id}>
                <td>{row.id}</td>
                <td>{row.position}</td>
                <td>{row.title}</td>
                <td>{title}</td>
                <td>{format}</td>
            </tr>);
    };
    let reducer = (acc, cur, index) => {
        let { tracks, format, title } = cur;
        let partialRender = parseToJSX(tracks, title, format);
        if (index !== 1)
            return [...partialRender, ...acc];
        else
            return partialRender;
    }
    let toRender = results.length === 1 ? parseToJSX(results[0].tracks, results[0].title, results[0].format) : results.reduce(reducer);
    return toRender;
})

//ideally, 2 methods, getHeaders(mode) & getBody(mode) with 4 other methods or components with render{MODE}Body calls
const DisplayResults = (props) => {
    var { ApiData, GetReleases, GetTracks } = props;
    const [results, setResults] = useState(null);
    const [mode, setMode] = useState(ARTIST);
    useEffect(() => {
        if (ApiData && ApiData.artists) {
            setMode(ARTIST);
            setResults(ApiData.artists);
        }
        if (ApiData && ApiData["releases"]) {
            setMode(ALBUMS);
            setResults(ApiData["releases"]);
        }
        if (ApiData && ApiData["media"]) {
            setMode(TRACKS);
            setResults(ApiData["media"]);//associate with url segment...
        }
    }, [ApiData]);

    if (!results) return null;

    var tbody = (results ?
        <tbody>
            {mode === ARTIST && results.map(row =>
                <tr key={row.id}>
                    <td><a className="action" onClick={() => GetReleases(row.id)}> {row.id}</a></td>
                    <td>{row.name}</td>
                    <td>{row.type}</td>
                    <td>{row.country}</td>
                    <td>{row.score}</td>
                </tr>)}
            {mode === ALBUMS && results.map(row =>
                <tr key={row.id}>
                    <td><a className="action" onClick={() => GetTracks(row.id)}> {row.id}</a></td>
                    <td>{row.title}</td>
                    <td>{row["primary-type"]}</td>
                    <td>{row["first-release-date"]}</td>
                </tr>)
            }
            {
                mode === TRACKS && parseTracks(results)
            }
        </tbody> : null);
    return <table>
        <thead>
            <tr>
                <th>ID</th>
                {mode === TRACKS && <th>Position</th>}
                <th>Name</th>
                {mode === ARTIST && <th>Country</th>}
                {mode === ARTIST && <th>Score</th>}
                {mode === ALBUMS && <th>Release date</th>}
                {mode === TRACKS && <th>Release Title</th>}
                {mode === TRACKS && <th>Release format</th>}
            </tr>
        </thead>
        {tbody}
    </table>

}

export default DisplayResults;