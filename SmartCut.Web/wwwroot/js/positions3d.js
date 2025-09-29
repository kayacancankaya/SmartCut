// wwwroot/js/positions3d.js
(function ()  {
    let scene, camera, renderer, controls, raycaster, mouse, tooltipDiv, animationId;
    let objects = [];
    let container;

    function ensureContainer() {
        container = document.getElementById('three-container');
        if (!container) {
            console.error("No #three-container element found. Add <div id='three-container'></div> to your page.");
            return false;
        }
        return true;
    }

    function colorFromId(id) {
        // simple deterministic color generator from numeric id
        const r = (id * 97) % 255;
        const g = (id * 53) % 255;
        const b = (id * 193) % 255;
        return `rgb(${r},${g},${b})`;
    }

    function initScene() {
        if (!ensureContainer()) return;

        // Renderer
        renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setPixelRatio(window.devicePixelRatio || 1);
        renderer.setSize(container.clientWidth, container.clientHeight);
        container.innerHTML = ''; // clear
        container.appendChild(renderer.domElement);

        // Camera
        const fov = 60;
        const aspect = container.clientWidth / container.clientHeight;
        camera = new THREE.PerspectiveCamera(fov, aspect, 0.1, 10000);
        camera.position.set(300, 300, 300);

        // Scene
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0xffffff);

        // Lights
        const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 0.6);
        hemi.position.set(0, 200, 0);
        scene.add(hemi);

        const dir = new THREE.DirectionalLight(0xffffff, 0.8);
        dir.position.set(300, 400, 200);
        scene.add(dir);

        // Grid and axes
        const grid = new THREE.GridHelper(1000, 50, 0x999999, 0xeeeeee);
        scene.add(grid);
        const axes = new THREE.AxesHelper(200);
        scene.add(axes);

        // Controls
        controls = new THREE.OrbitControls(camera, renderer.domElement);
        controls.enableDamping = true;
        controls.dampingFactor = 0.1;

        // Raycaster
        raycaster = new THREE.Raycaster();
        mouse = new THREE.Vector2();

        // Tooltip div
        tooltipDiv = document.getElementById('three-tooltip');
        if (!tooltipDiv) {
            tooltipDiv = document.createElement('div');
            tooltipDiv.id = 'three-tooltip';
            container.appendChild(tooltipDiv);
        }

        // Resize handling
        window.addEventListener('resize', onWindowResize);
        container.addEventListener('mousemove', onMouseMove);
    }

    function onWindowResize() {
        if (!container || !camera || !renderer) return;
        camera.aspect = container.clientWidth / container.clientHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(container.clientWidth, container.clientHeight);
    }

    function onMouseMove(event) {
        const rect = container.getBoundingClientRect();
        mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;
    }

    function clearScene() {
        if (!scene) return;
        objects.forEach(obj => {
            scene.remove(obj);
            if (obj.geometry) obj.geometry.dispose();
            if (obj.material) {
                if (Array.isArray(obj.material)) obj.material.forEach(m => m.dispose());
                else obj.material.dispose();
            }
        });
        objects = [];
    }

    function animate() {
        animationId = requestAnimationFrame(animate);
        controls.update();

        // hover check
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(objects, true);
        if (intersects.length > 0) {
            const first = intersects[0].object.userData || intersects[0].object.parent?.userData || {};
            if (first) {
                tooltipDiv.style.display = 'block';
                tooltipDiv.style.left = ((((intersects[0].point.x - camera.position.x) * 0)) + (eventClientX_safe())) + 'px';
                // Position tooltip near mouse pointer using container's rect
                const rect = container.getBoundingClientRect();
                tooltipDiv.style.left = (lastMouseX - rect.left) + 'px';
                tooltipDiv.style.top = (lastMouseY - rect.top) + 'px';
                tooltipDiv.innerHTML = first.tooltip || '';
            }
        } else {
            tooltipDiv.style.display = 'none';
        }

        renderer.render(scene, camera);
    }

    // helpers to avoid 'event' not defined: we cache last mouse client coords
    let lastMouseX = 0, lastMouseY = 0;
    function eventClientX_safe() { return lastMouseX; }
    function eventClientY_safe() { return lastMouseY; }
    document.addEventListener('mousemove', (e) => { lastMouseX = e.clientX; lastMouseY = e.clientY; });

    // Build boxes from positions array
    function buildFromPositions(positions) {
        clearScene();

        // Optionally compute bounds to center camera
        let min = { x: Infinity, y: Infinity, z: Infinity }, max = { x: -Infinity, y: -Infinity, z: -Infinity };
        positions.forEach(p => {
            const x1 = p.x, y1 = p.y, z1 = p.z;
            const x2 = p.x + p.ex, y2 = p.y + p.ey, z2 = p.z + p.ez;
            min.x = Math.min(min.x, x1); min.y = Math.min(min.y, y1); min.z = Math.min(min.z, z1);
            max.x = Math.max(max.x, x2); max.y = Math.max(max.y, y2); max.z = Math.max(max.z, z2);
        });

        positions.forEach(p => {
            // Some defensive fixes if fields are string typed
            const px = Number(p.x) || 0;
            const py = Number(p.y) || 0;
            const pz = Number(p.z) || 0;
            const ex = Math.abs(Number(p.ex) || 1);
            const ey = Math.abs(Number(p.ey) || 1);
            const ez = Math.abs(Number(p.ez) || 1);

            const geometry = new THREE.BoxGeometry(ex, ey, ez);
            const mat = new THREE.MeshStandardMaterial({
                color: colorFromId(Number(p.cutEntryId || p.CutEntryId || 0)),
                opacity: 0.85,
                transparent: true
            });
            const mesh = new THREE.Mesh(geometry, mat);

            // Move so that X,Y,Z are min-corner: Three BoxGeometry is centered at origin, so translate by half sizes
            mesh.position.set(px + ex / 2, py + ey / 2, pz + ez / 2);

            // store tooltip data
            mesh.userData = {
                tooltip: `Id: ${p.id || p.Id}<br/>CutEntry: ${p.cutEntryId || p.CutEntryId}<br/>OrderLine: ${p.orderLineId || p.OrderLineId}`
            };

            scene.add(mesh);
            objects.push(mesh);

            // add wireframe / outline
            const geo2 = new THREE.EdgesGeometry(geometry);
            const line = new THREE.LineSegments(geo2, new THREE.LineBasicMaterial({ color: 0x222222 }));
            line.position.copy(mesh.position);
            scene.add(line);
            objects.push(line);
        });

        // center camera on bounding box
        if (min.x !== Infinity) {
            const center = {
                x: (min.x + max.x) / 2,
                y: (min.y + max.y) / 2,
                z: (min.z + max.z) / 2
            };
            controls.target.set(center.x, center.y, center.z);
            camera.lookAt(center.x, center.y, center.z);

            // set distance so everything is visible
            const sizeX = (max.x - min.x) || 1;
            const sizeY = (max.y - min.y) || 1;
            const sizeZ = (max.z - min.z) || 1;
            const diagonal = Math.sqrt(sizeX * sizeX + sizeY * sizeY + sizeZ * sizeZ);
            camera.position.set(center.x + diagonal, center.y + diagonal, center.z + diagonal);
        }
    }

    // Public function called by Blazor
    window.renderPositions3D = function (positions) {
        try {
            if (!ensureContainer()) return;
            if (!scene) initScene();
            // normalize field names: allow PascalCase or camelCase
            const normalized = (positions || []).map(p => ({
                id: p.Id ?? p.id,
                orderLineId: p.OrderLineId ?? p.orderLineId,
                cutEntryId: p.CutEntryId ?? p.cutEntryId,
                x: Number(p.X ?? p.x ?? 0),
                y: Number(p.Y ?? p.y ?? 0),
                z: Number(p.Z ?? p.z ?? 0),
                ex: Number(p.Ex ?? p.ex ?? 1),
                ey: Number(p.Ey ?? p.ey ?? 1),
                ez: Number(p.Ez ?? p.ez ?? 1)
            }));
            buildFromPositions(normalized);
            if (!animationId) animate();
        } catch (err) {
            console.error('renderPositions3D error', err);
        }
    };

    window.destroyPositions3D = function () {
        if (animationId) {
            cancelAnimationFrame(animationId);
            animationId = null;
        }
        if (controls) {
            controls.dispose();
            controls = null;
        }
        if (renderer) {
            renderer.dispose();
            if (renderer.domElement && renderer.domElement.parentNode) renderer.domElement.parentNode.removeChild(renderer.domElement);
            renderer = null;
        }
        if (scene) {
            clearScene();
            scene = null;
        }
        if (window.removeEventListener) {
            window.removeEventListener('resize', onWindowResize);
            container && container.removeEventListener('mousemove', onMouseMove);
        }
        const tt = document.getElementById('three-tooltip');
        if (tt && tt.parentNode) tt.parentNode.removeChild(tt);
    };

})();
